using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PokerTracker3000.Common;
using PokerTracker3000.Common.FileUtilities;
using PokerTracker3000.GameComponents;

namespace PokerTracker3000.GameSession
{
    public class GameSettings : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private CurrencyType _currencyType = CurrencyType.SwedishKrona;
        private decimal _defaultBuyInAmount = 500;
        private decimal _defaultAddOnAmount = 500;
        private int _defaultStageLengthSeconds = 20 * 60;
        #endregion

        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyType CurrencyType
        {
            get => _currencyType;
            set => SetProperty(ref _currencyType, value);
        }

        public decimal DefaultBuyInAmount
        {
            get => _defaultBuyInAmount;
            set => SetProperty(ref _defaultBuyInAmount, value);
        }

        public decimal DefaultAddOnAmount
        {
            get => _defaultAddOnAmount;
            set => SetProperty(ref _defaultAddOnAmount, value);
        }

        public int DefaultStageLengthSeconds
        {
            get => _defaultStageLengthSeconds;
            set => SetProperty(ref _defaultStageLengthSeconds, value);
        }
        #endregion

        #region Private fields
        private record GameSetup(GameSettings Settings, List<GameStage> Stages);
        #endregion

        #region Public methods

        public bool TrySave(GameStagesManager stageManager, string filePath, out string resultMessage)
        {
            List<GameStage> stages = [];
            if (stageManager.TryGetStageByIndex(0, out var stage))
            {
                stages.Add(stage!);
                while (stageManager.TryGetNextStage(stage!, out var next))
                {
                    stages.Add(next!);
                    stage = next;
                }
            }

            var setup = new GameSetup(this, stages);
            var (success, fullPath, e) = setup.SerializeWriteToJsonFile(filePath);
            resultMessage = success ? $"Settings saved to '{Path.GetFileName(fullPath)}'!" : $"Save failed - {e!.Message}";
            return success;
        }

        public bool TryLoadFromFile(GameStagesManager stageManager, string filePath, out string resultMessage)
        {
            FileTextReader reader = new(filePath);
            if (!reader.SuccessfulRead)
            {
                resultMessage = $"Reading settings failed - {reader.ReadException!.Message}";
                return false;
            }

            var (setup, e) = reader.AllText.DeserializeJsonString<GameSetup>(convertSnakeCaseToPascalCase: true);
            if (e != default)
            {
                resultMessage = $"Reading settings failed - {e!.Message}";
                return false;
            }

            stageManager.SetStages(setup!.Stages);

            CurrencyType = setup.Settings.CurrencyType;
            DefaultBuyInAmount = setup.Settings.DefaultBuyInAmount;
            DefaultAddOnAmount = setup.Settings.DefaultAddOnAmount;
            DefaultStageLengthSeconds = setup.Settings.DefaultStageLengthSeconds;

            resultMessage = $"Settings read from '{Path.GetFileName(filePath)}!";
            CurrencyType = setup.Settings.CurrencyType;
            return true;
        }
        #endregion
    }
}
