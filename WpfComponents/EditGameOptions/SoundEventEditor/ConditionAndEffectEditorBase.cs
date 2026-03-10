using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using PokerTracker3000.Common;
using PokerTracker3000.GameSession;
using PokerTracker3000.Interfaces;

namespace PokerTracker3000.WpfComponents.EditGameOptions
{
    public abstract class ConditionAndEffectEditorBase : UserControl
    {
        #region Dependency properties
        public IInputRelay NavigationRelay
        {
            get => (IInputRelay)GetValue(NavigationRelayProperty);
            set => SetValue(NavigationRelayProperty, value);
        }
        public static readonly DependencyProperty NavigationRelayProperty = DependencyProperty.Register(
            nameof(NavigationRelay),
            typeof(IInputRelay),
            typeof(ConditionAndEffectEditorBase),
            new FrameworkPropertyMetadata(default));

        public NavigationManager NavigationManager
        {
            get { return (NavigationManager)GetValue(NavigationManagerProperty); }
            set { SetValue(NavigationManagerProperty, value); }
        }
        public static readonly DependencyProperty NavigationManagerProperty = DependencyProperty.Register(
            nameof(NavigationManager),
            typeof(NavigationManager),
            typeof(ConditionAndEffectEditorBase),
            new FrameworkPropertyMetadata(default));

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(ConditionAndEffectEditorBase),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region Events
        public event EventHandler? NewValidContentEvent;
        public event EventHandler? RemoveEvent;
        #endregion

        #region Protected methods
        protected void RaiseNewValidContentEvent()
        {
            NewValidContentEvent?.Invoke(this, EventArgs.Empty);
        }

        protected void RaiseRemoveEvent()
        {
            RemoveEvent?.Invoke(this, EventArgs.Empty);
        }

        protected static void PopulateOptionsList<T>(ObservableCollection<string> displayName, List<T> optionMap) where T : struct, Enum
        {
            foreach (var t in Enum.GetValues<T>())
            {
                var (attr, e) = t.GetCustomAttributeFromEnum<DescriptionAttribute>();
                if (e == default)
                {
                    displayName.Add(attr!.Description);
                    optionMap.Add(t);
                }
            }
        }
        #endregion
    }
}
