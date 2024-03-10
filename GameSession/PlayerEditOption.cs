using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PokerTracker3000.Common;

namespace PokerTracker3000.GameSession
{
    public class PlayerEditOption : ObservableObject
    {
        public enum EditOption
        {
            [Description("")]
            None,

            [Description("Change name")]
            ChangeName,

            [Description("Change image")]
            ChangeImage,

            [Description("Move player")]
            Move,

            [Description("Save player")]
            Save,

            [Description("Load player")]
            Load,

            [Description("Add-on")]
            AddOn,

            [Description("Buy-in")]
            BuyIn,

            [Description("Eliminate")]
            Eliminate,

            [Description("Remove")]
            Remove
        }

        public enum OptionType
        {
            Default,
            Success,
            Cancel
        };

        #region Public properties

        #region Private fields
        private EditOption _option = EditOption.None;
        private string _name = string.Empty;
        private bool _isSelected = false;
        private bool _isAvailable = true;
        #endregion

        public EditOption Option
        {
            get => _option;
            private set => SetProperty(ref _option, value);
        }

        public string Name
        {
            get => _name;
            private set => SetProperty(ref _name, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set => SetProperty(ref _isAvailable, value);
        }

        public OptionType Type { get; init; }
        #endregion

        public PlayerEditOption(EditOption option, OptionType type = OptionType.Default, bool isSelected = false)
        {
            ChangeEditOption(option);
            Type = type;
            IsSelected = isSelected;
        }

        public void ChangeEditOption(EditOption option)
        {
            Option = option;
            var (attr, _) = Option.GetCustomAttributeFromEnum<DescriptionAttribute>();
            Name = attr!.Description;
        }
    }
}
