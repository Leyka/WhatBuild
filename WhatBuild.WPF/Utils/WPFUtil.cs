using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace WhatBuild.WPF.Utils
{
    public class WPFUtil
    {
        public static void ToggleVisibility(UIElement element)
        {
            element.Visibility = element.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }
}
