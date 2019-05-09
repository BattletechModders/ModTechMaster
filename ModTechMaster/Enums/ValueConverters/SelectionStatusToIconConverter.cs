﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace ModTechMaster.Enums.ValueConverters
{
    [ValueConversion(typeof(SelectionStatus), typeof(string))]
    public class SelectionStatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((SelectionStatus) value)
            {
                case SelectionStatus.Unselected:
                    return "./Resources/Images/trash.png";
                case SelectionStatus.PartiallySelected:
                    return "./Resources/Images/minus.png";
                case SelectionStatus.Selected:
                    return "./Resources/Images/plus.png";
            }

            return "./Resources/Images/cancel.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}