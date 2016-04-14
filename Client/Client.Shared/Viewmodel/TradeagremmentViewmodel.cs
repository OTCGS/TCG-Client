using Client.Game.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Client.Viewmodel
{
    class TradeagreementViewmodel : DependencyObject
    {
        public TradeAgreement Agreement { get; }

        public ObservableCollection<CardViewmodel> CardsGiven { get; } = new ObservableCollection<CardViewmodel>();
        public ObservableCollection<CardViewmodel> CardsTaken { get; } = new ObservableCollection<CardViewmodel>();

        public TradeagreementViewmodel(TradeAgreement agreement)
        {
            this.Agreement = agreement;
            Load();
        }

        private async void Load()
        {

            var cardsgiven = await Task.WhenAll(Agreement.CardsGiven.Select(async x =>
            {
                var vm = new CardViewmodel();
                await vm.LoadData(x);
                return vm;
            }));
            foreach (var item in cardsgiven)
                CardsGiven.Add(item);


            var cardstaken = await Task.WhenAll(Agreement.CardsTaken.Select(async x =>
            {
                var vm = new CardViewmodel();
                await vm.LoadData(x);
                return vm;
            }));
            foreach (var item in cardstaken)
                CardsTaken.Add(item);

        }
    }
}
