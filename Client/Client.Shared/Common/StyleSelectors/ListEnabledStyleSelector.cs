using Client.Viewmodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Client.Common.StyleSelectors
{
    class ListEnabledStyleSelector : StyleSelector
    {

        public event Func<DeckViewmodel, Task<IEnumerable<string>>> Filter;


        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            var c = container as Control;
            c.IsEnabled = false;
            var vm = item as Viewmodel.DeckViewmodel;
            vm.IncreaseLoading();
            UpdateEnabled(vm, c);

            var style = base.SelectStyleCore(item, container);
            style = new Style();
            style.TargetType = typeof(ListViewItem);
            var setter = new Setter();
            setter.Property = Control.IsEnabledProperty;
            setter.Value = false;
            style.Setters.Add(setter);
            return style;
        }

        private async void UpdateEnabled(DeckViewmodel vm, Control c)
        {
            await vm.LodingWaiter;
            try
            {
                Logger.Information("Waiting for Filter Progression");
                var erg = await Filter(vm);
                Logger.Information("Filter Ready");
                if (!erg.Any())
                    c.IsEnabled = true;
                else
                    vm.Errors = erg;

            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
            finally
            {
                vm.DecreaseLoading();
                Logger.Information("Loding deactivated.");
            }
        }
    }
}
