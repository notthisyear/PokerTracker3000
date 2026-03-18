using System;
using System.ComponentModel;
using PokerTracker3000.Common;

namespace PokerTracker3000.GameSession
{
    public class ButtonOptionModel : SelectableEntity
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

            [Description("Set chip lead")]
            SetAsChipLead,

            [Description("Remove chip lead")]
            RemoveAsChipLead,

            [Description("Add-on")]
            AddOn,

            [Description("Buy-in")]
            BuyIn,

            [Description("Eliminate")]
            Eliminate,

            [Description("Remove")]
            Remove,

            [Description("+1000")]
            Add1000,

            [Description("+100")]
            Add100,

            [Description("+10")]
            Add10,

            [Description("+1")]
            Add1,

            [Description("-1000")]
            Remove1000,

            [Description("-100")]
            Remove100,

            [Description("-10")]
            Remove10,

            [Description("-1")]
            Remove1,

            [Description("OK")]
            Ok,

            [Description("Add stage")]
            AddStage,

            [Description("Remove stage")]
            RemoveStage,
        }

        public enum OptionType
        {
            Default,
            Info,
            Success,
            Cancel
        };

        #region Public properties

        #region Private fields
        private EditOption _option = EditOption.None;
        private string _name = string.Empty;
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

        public bool IsAvailable
        {
            get => _isAvailable;
            set => SetProperty(ref _isAvailable, value);
        }

        public Action? ButtonAction { get; set; }

        public OptionType Type { get; init; }
        #endregion

        public ButtonOptionModel(EditOption option, OptionType type = OptionType.Default, bool isSelected = false, bool isAvailable = true)
        {
            ChangeEditOption(option);
            Type = type;
            IsSelected = isSelected;
            IsAvailable = isAvailable;
        }

        public ButtonOptionModel(string name, Action? buttonAction = default, OptionType type = OptionType.Default, bool isSelected = false, bool isAvailable = true)
        {
            Name = name;
            ButtonAction = buttonAction;
            Type = type;
            IsSelected = isSelected;
            IsAvailable = isAvailable;

        }
        public void ChangeEditOption(EditOption option)
        {
            if (Option == option)
                return;

            Option = option;
            var (attr, _) = Option.GetCustomAttributeFromEnum<DescriptionAttribute>();
            Name = attr!.Description;
        }
    }
}
