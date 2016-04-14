using Client.Game.Data;
using Client.Viewmodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Client.Common.Converters
{
    public class TradeagreementToTradeagreementViewmodel : ConverterWithDefinedTargetType
    {
        public override IEnumerable<Type> InputTypes { get; } = new Type[] { typeof(CardInstance), typeof(ObservableCollection<CardInstance>), typeof(IEnumerable<CardInstance>) };

        public override Type ReturnType { get; } = typeof(CardViewmodel);

        protected override object InternalCovert(object value, object parameter, string language)
        {
            if (value is CardInstance)
            {
                var vm = new Viewmodel.CardViewmodel();
                vm.LoadData(value as CardInstance);
                return vm;
            }
            else if (value is ObservableCollection<CardInstance>)
            {
                var oc = value as ObservableCollection<CardInstance>;
                var outOc = new ObservableCollection<CardViewmodel>(oc.Select(x =>
                {
                    var vm = new Viewmodel.CardViewmodel();
                    vm.LoadData(value as CardInstance);
                    return vm;

                }));

                oc.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
                {
                    if (e.NewItems != null)
                        foreach (CardInstance item in e.NewItems)
                        {
                            var vm = new Viewmodel.CardViewmodel();
                            vm.LoadData(value as CardInstance);
                            outOc.Add(vm);
                        }
                    if (e.OldItems != null)
                        foreach (CardInstance item in e.OldItems)
                        {

                            var index = outOc.IndexOf(x => x.CardInstance.Equals(item));
                            if (index != -1)
                                outOc.RemoveAt(index);
                        }
                };


                return outOc;
            }
            else if (value is IEnumerable<CardInstance>)
            {
                var en = value as IEnumerable<CardInstance>;
                return en.Select(x =>
                {
                    var vm = new Viewmodel.CardViewmodel();
                    vm.LoadData(x as CardInstance);
                    return vm;

                });
                return en;

            }

            return null;
        }



        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is CardViewmodel)
                return (value as CardViewmodel).CardInstance;
            return null;
        }


    }
}