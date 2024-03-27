using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PokerTracker3000.GameSession
{
    public class SideMenuOptionModel : ObservableObject
    {
        #region Public properties

        #region Backing fields
        private string _optionText = string.Empty;
        private string _descriptionText = string.Empty;
        private bool _isHighlighted = false;
        private bool _isSelected = false;
        private bool _isAvailable = true;
        private string _unavaliableDescriptionText = string.Empty;
        #endregion

        public string OptionText
        {
            get => _optionText;
            set => SetProperty(ref _optionText, value);
        }

        public string DescriptionText
        {
            get => _descriptionText;
            set => SetProperty(ref _descriptionText, value);
        }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => SetProperty(ref _isHighlighted, value);
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

        public string UnavaliableDescriptionText
        {
            get => _unavaliableDescriptionText;
            set => SetProperty(ref _unavaliableDescriptionText, value);
        }

        public bool HasSubOptions { get; init; }

        public List<SideMenuOptionModel> SubOptions { get; init; } = [];

        public int Id { get; init; }

        public bool IsSubOption { get; init; } = false;

        public Action<SideMenuOptionModel>? OnOpenAction { get; init; } = default;

        public Action<SideMenuOptionModel>? OptionAction { get; init; } = default;
        #endregion
    }
}
