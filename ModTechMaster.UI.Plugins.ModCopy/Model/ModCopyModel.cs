﻿namespace ModTechMaster.UI.Plugins.ModCopy.Model
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using Annotations;
    using ModTechMaster.Core.Interfaces.Services;
    using Nodes;

    public class ModCopyModel : INotifyPropertyChanged
    {
        private IMtmTreeViewItem currentSelectedItem;
        private ModCopySettings _settings;

        public ModCopyModel()
        {
        }

        public IMtmTreeViewItem CurrentSelectedItem
        {
            get => this.currentSelectedItem;
            set
            {
                if (object.Equals(value, this.currentSelectedItem))
                {
                    return;
                }

                this.currentSelectedItem = value;
                this.OnPropertyChanged();
            }
        }

        public ModCopySettings Settings
        {
            get => _settings;
            set
            {
                if (Equals(value, _settings)) return;
                _settings = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.CurrentSelectedItem = e.NewValue as IMtmTreeViewItem;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}