using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PokerTracker3000.Common.FileUtilities;
using PokerTracker3000.GameSession.Sound;

namespace PokerTracker3000.Common
{
    internal static class Settings
    {
        public static ApplicationSettings App => s_isInitialized ? s_appSettings! : throw new InvalidOperationException("Settings not initialized");

        private static bool s_isInitialized = false;

        private static ApplicationSettings? s_appSettings;

        public static void Initalize(string settingsName)
        {
            if (s_isInitialized)
                return;

            var reader = new FileTextReader(GetFullPathToResource(settingsName));
            if (!reader.SuccessfulRead)
                throw reader.ReadException!;

            var (app, e) = reader.AllText.DeserializeJsonString<ApplicationSettings>(settings:
                new JsonSerializerSettings()
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    },
                    Converters = { new StringEnumConverter() }
                });

            if (e != default)
                throw e;

            app!.DefaultPlayerImagePath = GetFullPathToResource(app!.DefaultPlayerImagePath, subfolder: "Images");
            foreach (var entry in app!.BuiltInEffects)
                app!.BuiltInEffects[entry.Key] = GetFullPathToResource(entry.Value);

            s_appSettings = app!;
            s_isInitialized = true;
        }

        private static string GetFullPathToResource(string fileName, string resourceFolder = "Resources", string subfolder = "")
        {
            var pathToExecutable = AppContext.BaseDirectory;
            if (string.IsNullOrEmpty(subfolder))
                return Path.Combine(pathToExecutable, resourceFolder, fileName);
            else
                return Path.Combine(pathToExecutable, resourceFolder, subfolder, fileName);
        }
    }

    public class ApplicationSettings
    {
        public string ClientId { get; init; } = string.Empty;

        public int LocalHttpListenerPort { get; init; }

        public int PkceAuthorizationVerifierLength { get; init; }

        public string DefaultPlayerImagePath { get; set; } = string.Empty;

        public Dictionary<BuiltInSoundEffectType, string> BuiltInEffects { get; set; } = [];
    }
}
