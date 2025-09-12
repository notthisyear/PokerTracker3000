using System;
using System.Collections.Generic;
using System.IO;
using PokerTracker3000.GameSession;

namespace PokerTracker3000.Common
{
    internal static class SessionSaveFile
    {
        public static bool TryWrite(string filePath,
                                    GameSessionManager sessionManager,
                                    out string resultMessage)
        {
            Dictionary<int, (string imagePath, long size)> playerImages = [];
            List<PlayerSpot> playerSpots = [];
            var imageIndex = 0;
            foreach (var spot in sessionManager.PlayerSpots)
            {
                if (!spot.HasPlayerData)
                    continue;

                var imageInfo = new FileInfo(spot.PlayerData.PathToImage);

                playerImages.Add(imageIndex, (spot.PlayerData.PathToImage, imageInfo.Length));

                var spotToWrite = new PlayerSpot
                {
                    SpotIndex = spot.SpotIndex,
                    IsEliminated = spot.IsEliminated
                };

                spotToWrite.AddPlayer(spot.PlayerData);
                spotToWrite.PlayerData.EmbeddedImageIndex = imageIndex++;
                spotToWrite.PlayerData.PathToImage = Path.GetFileName(spot.PlayerData.PathToImage);
                playerSpots.Add(spotToWrite);
            }

            var pathRoot = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(pathRoot))
            {
                resultMessage = "Save failed - could not path root";
                return false;
            }

            var settingsPath = Path.Combine(pathRoot, Path.GetRandomFileName());
            if (!sessionManager.TrySaveGameSettings(settingsPath, out resultMessage, false))
                return false;

            var tableConfigurationPath = Path.Combine(pathRoot, Path.GetRandomFileName());
            if (!GameSessionManager.TrySaveTableConfiguration(tableConfigurationPath, playerSpots, out resultMessage, addExtension: false))
                return false;

            var success = true;
            try
            {
                using BinaryWriter writer = new(new FileStream(filePath, FileMode.Create, FileAccess.Write));

                // Write a single byte to indicate little or big endian
                writer.Write(BitConverter.IsLittleEndian ? (byte)1 : (byte)0);

                // Write the current clock info
                writer.Write(BitConverter.GetBytes(sessionManager.Clock.NumberOfSeconds));

                // Write the current stage number
                writer.Write(BitConverter.GetBytes(sessionManager.StageManager.CurrentStage?.Number ?? -1));

                // Write the settings length and then the settings
                var settingsLength = new FileInfo(settingsPath).Length;

                writer.Write(BitConverter.GetBytes(settingsLength));
                using (BinaryReader reader = new(new FileStream(settingsPath, FileMode.Open, FileAccess.Read)))
                {
                    var settings = new byte[settingsLength];
                    reader.Read(settings, 0, (int)settingsLength);
                    writer.Write(settings);
                }

                // Write the table configuration length and then the config
                var tableConfigurationLength = new FileInfo(tableConfigurationPath).Length;

                writer.Write(BitConverter.GetBytes(tableConfigurationLength));
                using (BinaryReader reader = new(new FileStream(tableConfigurationPath, FileMode.Open, FileAccess.Read)))
                {
                    var tableConfiguration = new byte[tableConfigurationLength];
                    reader.Read(tableConfiguration, 0, (int)tableConfigurationLength);
                    writer.Write(tableConfiguration);
                }

                foreach (var imageData in playerImages)
                {
                    // Write the image id, image length and then image bytes
                    writer.Write(BitConverter.GetBytes(imageData.Key));
                    writer.Write(BitConverter.GetBytes(imageData.Value.size));
                    using BinaryReader reader = new(new FileStream(imageData.Value.imagePath, FileMode.Open, FileAccess.Read));
                    var image = new byte[imageData.Value.size];
                    reader.Read(image, 0, (int)imageData.Value.size);
                    writer.Write(image);
                }
            }
            catch (Exception e)
            {
                success = false;
                resultMessage = $"Save failed - {e}";
            }

            // The files can be used by another process, which will make deletion operation fail


            try
            {
                File.Delete(settingsPath);
                File.Delete(tableConfigurationPath);
            }
            catch (IOException) { }

            if (!success && File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (IOException) { }
            }
            else
            {
                resultMessage = $"Configuration saved to '{Path.GetFileName(filePath)}";
            }
            return success;
        }

        public static bool TryLoad(string filePath,
                                    GameSessionManager sessionManager,
                                    out int clockNumberOfSeconds,
                                    out int currentStageNumber,
                                    out string resultMessage)
        {
            clockNumberOfSeconds = -1;
            currentStageNumber = -1;

            using BinaryReader reader = new(new FileStream(filePath, FileMode.Open, FileAccess.Read));
            var success = true;
            try
            {
                // Read the endianess byte
                var fileIsLittleEndian = reader.ReadByte() == (byte)0x01;

                if (fileIsLittleEndian != BitConverter.IsLittleEndian)
                {
                    resultMessage = "Loading session failed - file is corrupt";
                    return false;
                }

                // Read the number of seconds on clock
                clockNumberOfSeconds = reader.ReadInt32();

                // Read the current stage number
                currentStageNumber = reader.ReadInt32();

                // Read the settings length
                var settingsLength = reader.ReadInt64();

                // Read in the settings and flush to a temporary
                // file so that we can use the standard framework
                var settingPath = Path.GetTempFileName();
                using (BinaryWriter writer = new(new FileStream(settingPath, FileMode.OpenOrCreate, FileAccess.Write)))
                {
                    var settings = new byte[settingsLength];
                    reader.Read(settings, 0, (int)settingsLength);
                    writer.Write(settings);
                }

                if (!sessionManager.TryLoadGameSettingsFromFile(settingPath, out resultMessage))
                    return false;

                // Read the table configuration 
                var tableConfigurationLength = reader.ReadInt64();

                // Read in the configuration and flush to a temporary
                // file so that we can use the standard framework
                var tableConfigurationPath = Path.GetTempFileName();
                using (BinaryWriter writer = new(new FileStream(tableConfigurationPath, FileMode.OpenOrCreate, FileAccess.Write)))
                {
                    var tableConfiguration = new byte[tableConfigurationLength];
                    reader.Read(tableConfiguration, 0, (int)tableConfigurationLength);
                    writer.Write(tableConfiguration);
                }

                // Read all the images into temporary files
                Dictionary<int, string> playerImages = [];
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    // Read the image ID
                    var imageId = reader.ReadInt32();

                    // Read the image length
                    var imageLength = reader.ReadInt64();

                    // Read in and flush the image to a file
                    var imagePath = Path.GetTempFileName();
                    using BinaryWriter writer = new(new FileStream(imagePath, FileMode.OpenOrCreate, FileAccess.Write));

                    var image = new byte[imageLength];
                    reader.Read(image, 0, (int)imageLength);
                    writer.Write(image);

                    playerImages.Add(imageId, imagePath);
                }

                sessionManager.TryLoadTableConfigurationFromFile(tableConfigurationPath, out resultMessage, playerImages);
            }
            catch (Exception e)
            {
                success = false;
                resultMessage = $"Load failed - {e}";
            }

            resultMessage = $"Configuration loaded from '{Path.GetFileName(filePath)}";
            return success;
        }
    }
}
