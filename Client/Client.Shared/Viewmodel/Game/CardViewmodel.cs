using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Client.Viewmodel.Game
{
    public class CardViewmodel : DependencyObject
    {
        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        // Using a DependencyProperty as the backing store for X.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(CardViewmodel), new PropertyMetadata(0.0, PositionChanged));

        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Y.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(CardViewmodel), new PropertyMetadata(0.0, PositionChanged));

        public int Z
        {
            get { return (int)GetValue(ZProperty); }
            set { SetValue(ZProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Z.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZProperty =
            DependencyProperty.Register("Z", typeof(int), typeof(CardViewmodel), new PropertyMetadata(0));

        private static void PositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as CardViewmodel;
            //me.Margin = new Thickness(me.X, me.Y, 0, 0);
        }

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Angle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(CardViewmodel), new PropertyMetadata(0.0));

        public bool FaceUp
        {
            get { return (bool)GetValue(FaceUpProperty); }
            set { SetValue(FaceUpProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FaceUp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FaceUpProperty =
            DependencyProperty.Register("FaceUp", typeof(bool), typeof(CardViewmodel), new PropertyMetadata(false));

        public ImageSource FrontImage
        {
            get { return (ImageSource)GetValue(FrontImageProperty); }
            set { SetValue(FrontImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FrontImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FrontImageProperty =
            DependencyProperty.Register("FrontImage", typeof(ImageSource), typeof(CardViewmodel), new PropertyMetadata(null));

        public ImageSource BackImage
        {
            get { return (ImageSource)GetValue(BackImageProperty); }
            set { SetValue(BackImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackImageProperty =
            DependencyProperty.Register("BackImage", typeof(ImageSource), typeof(CardViewmodel), new PropertyMetadata(null));

        //public Thickness Margin
        //{
        //    get { return (Thickness)GetValue(MarginProperty); }
        //    set { SetValue(MarginProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Margin.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty MarginProperty =
        //    DependencyProperty.Register("Margin", typeof(Thickness), typeof(CardViewmodel), new PropertyMetadata(new Thickness()));

        public MentalCardGame.Card Card
        {
            get { return (MentalCardGame.Card)GetValue(CardProperty); }
            set { SetValue(CardProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Card.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CardProperty =
            DependencyProperty.Register("Card", typeof(MentalCardGame.Card), typeof(CardViewmodel), new PropertyMetadata(null, CardChanged));

        private static void CardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as CardViewmodel;
            var c = e.NewValue as MentalCardGame.Card;
            me.FaceUp = !c.Type.HasValue;
        }
    }
}