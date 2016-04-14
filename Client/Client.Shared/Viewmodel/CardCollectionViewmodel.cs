using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using Client.Game.Data;
using System.Collections.ObjectModel;

namespace Client.Viewmodel
{
    public class CardCollectionViewmodel : DependencyObject
    {

        public ObservableCollection<CardViewmodel> Cards { get; } = new ObservableCollection<CardViewmodel>();

        public Task LoadingWaiter => loadingTaskSource.Task;
        private TaskCompletionSource<object> loadingTaskSource = new TaskCompletionSource<object>();

        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Loading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register(nameof(Loading), typeof(bool), typeof(CardCollectionViewmodel), new PropertyMetadata(false));

        public Network.User User
        {
            get { return (Network.User)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        // Using a DependencyProperty as the backing store for User.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserProperty =
            DependencyProperty.Register(nameof(User), typeof(Network.User), typeof(CardCollectionViewmodel), new PropertyMetadata(null, UserChanged));

        private async static void UserChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as CardCollectionViewmodel;
            if (e.NewValue != null)
            {
                if (me.loadingTaskSource.Task.IsCompleted)
                    me.loadingTaskSource = new TaskCompletionSource<object>();
                await me.LoadData(e.NewValue as Network.User);
                me.loadingTaskSource.SetResult(null);
            }
        }

        public CardCollectionViewmodel()
        {

        }



        private async Task LoadData(Network.User u)
        {
            //Func<byte[], Task<byte[]>> sign = b => (u.PublicKey as Security.IPrivateKey).Sign(b);
            //var cardInstance = new CardInstance() { CardDataId = Guid.NewGuid(), Creator = new PublicKey() { Exponent = u.PublicKey.Exponent, Modulus = u.PublicKey.Modulus } };
            //cardInstance.Id = Guid.NewGuid();
            //cardInstance.Signature = await sign(cardInstance.Bytes().ToArray());
            //await global::Game.TransactionMap.Graph.Trade(u.PublicKey, u.PublicKey, new CardInstance[0], new CardInstance[] { cardInstance }, sign, sign);

            //await System.Threading.Tasks.Task.Delay(1000);

            Loading = true;
            var cardsIds = await global::Game.TransactionMap.Graph.GetCardsOf(u.PublicKey);
            var cardInstances = (await Task.WhenAll(cardsIds.Select(x => DDR.GetCardInstances(x)))) as IEnumerable<CardInstance>;
            cardInstances = cardInstances.Where(x => x != null);
            var toAdd = await Task.WhenAll(cardInstances.Select(async instance =>
            {
                var vm = new CardViewmodel();
                await vm.LoadData(instance);
                return vm;
            }));

            Cards.UpdateCollection(toAdd);

            Loading = false;

        }

        internal Task Reload()
        {
            return LoadData(this.User);
        }
    }

    public class CardViewmodel : DependencyObject
    {

        public ServerId ServerId
        {
            get { return (ServerId)GetValue(ServerIdProperty); }
            private set { SetValue(ServerIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ServerId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ServerIdProperty =
            DependencyProperty.Register(nameof(ServerId), typeof(ServerId), typeof(CardViewmodel), new PropertyMetadata(null));




        public CardData CardData
        {
            get { return (CardData)GetValue(CardDataProperty); }
            private set { SetValue(CardDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CardData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CardDataProperty =
            DependencyProperty.Register(nameof(CardData), typeof(CardData), typeof(CardViewmodel), new PropertyMetadata(null));




        public CardInstance CardInstance
        {
            get { return (CardInstance)GetValue(CardInstanceProperty); }
            private set { SetValue(CardInstanceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CardInstance.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CardInstanceProperty =
            DependencyProperty.Register(nameof(CardInstance), typeof(CardInstance), typeof(CardViewmodel), new PropertyMetadata(null));





        public Uri Image
        {
            get { return (Uri)GetValue(ImageProperty); }
            private set { SetValue(ImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(nameof(Image), typeof(Uri), typeof(CardViewmodel), new PropertyMetadata(null));




        public DDR.Trust IsTrustWorthy
        {
            get { return (DDR.Trust)GetValue(IsTrustWorthyProperty); }
            set { SetValue(IsTrustWorthyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTrustWorthy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTrustWorthyProperty =
            DependencyProperty.Register("IsTrustWorthy", typeof(DDR.Trust), typeof(CardViewmodel), new PropertyMetadata(null));



        public async Task LoadData(UuidServer id)
        {
            await LoadData(await DDR.GetCardInstances(id));
        }
        public async Task LoadData(CardInstance instance)
        {
            Logger.Assert(instance != null, "cardinstance war null");
            if (instance == null)
                return;
            CardInstance = instance;
            CardData = await DDR.GetCardData(instance.CardDataId, instance.Creator);
            ServerId = await DDR.GetServerId(instance.Creator);
            Image = await DDR.GetCardDataImageUri(CardData.ImageId, CardData.Creator);
            IsTrustWorthy = await DDR.GetServerTrust(ServerId.Key);
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = obj as CardViewmodel;



            return other.CardInstance.Equals(this.CardInstance);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return this.CardInstance.GetHashCode();
        }

    }
}
