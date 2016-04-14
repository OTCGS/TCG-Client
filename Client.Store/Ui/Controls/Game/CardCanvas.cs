using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Client.Store.Ui.Controls.Game
{
    class CardCanvas :ItemsControl
    {

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            FrameworkElement source = element as FrameworkElement;
            source.SetBinding(Canvas.ZIndexProperty, new Binding { Path = new PropertyPath("Z") });
        }
    }
}
