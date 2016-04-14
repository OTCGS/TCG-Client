using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Client.Store.Common
{
    public static class ValidationExtension
    {
        public static bool GetIsValid(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsValidProperty);
        }

        public static void SetIsValid(DependencyObject obj, bool value)
        {
            obj.SetValue(IsValidProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsValid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.RegisterAttached("IsValid", typeof(bool), typeof(ValidationExtension), new PropertyMetadata(true, validChanged));

        public static Style GetInvalidStyle(DependencyObject obj)
        {
            return (Style)obj.GetValue(InvalidStyleProperty);
        }

        public static void SetInvalidStyle(DependencyObject obj, Style value)
        {
            obj.SetValue(InvalidStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for InvalidStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InvalidStyleProperty =
            DependencyProperty.RegisterAttached("InvalidStyle", typeof(Style), typeof(ValidationExtension), new PropertyMetadata(null));

        private static void validChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var style = GetInvalidStyle(d);
            var ui = d as FrameworkElement;
            var v = (bool)e.NewValue;
            if (v)
            {
                ui.Style = null;
            }
            else
            {
                ui.Style = style;
            }
            ui.UpdateLayout();
        }
    }
}