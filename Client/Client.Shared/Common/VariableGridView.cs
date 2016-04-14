using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Client.Common
{
    public class VariableGridView : GridView
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            var model = item as MenueItemModel;
            if (model!=null && element is UIElement)
            {
                VariableSizedWrapGrid.SetColumnSpan(element as UIElement, model.Width);
                VariableSizedWrapGrid.SetRowSpan(element as UIElement, model.Height);
            }
        }
    }

    public class MenueItemModel
    {
        public String Title { get; set; }

        public UIElement Icon { get; set; }
        public event Action OnClick;
        public int Width { get; set; }
        public int Height { get; set; }

        public void FierClick()
        {
            OnClick();
        }
    }
}
