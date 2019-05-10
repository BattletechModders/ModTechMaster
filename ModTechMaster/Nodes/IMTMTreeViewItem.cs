﻿using System.Collections.ObjectModel;
using ModTechMaster.Enums;

namespace ModTechMaster.Nodes
{
    public interface IMTMTreeViewItem
    {
        IMTMTreeViewItem Parent { get; }
        ObservableCollection<IMTMTreeViewItem> Children { get; }
        bool IsSelected { get; }
        bool IsExpanded { get; }
        bool HasCheck { get; }
        bool IsChecked { get; }
        SelectionStatus SelectionStatus { get; }
        ObjectStatus ObjectStatus { get; }
    }
}