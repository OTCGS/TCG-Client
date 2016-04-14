using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

// Die Elementvorlage "Steuerelement mit Vorlagen" ist unter http://go.microsoft.com/fwlink/?LinkId=234235 dokumentiert.

namespace Client.Store.Ui.Controls.Game
{
    public sealed class CardControl : Control
    {
        private readonly CompositeTransform transform = new CompositeTransform();

        public CardControl()
        {
            this.DefaultStyleKey = typeof(CardControl);
            this.Loaded += FrameworkElement_Loaded;
            this.RenderTransform = transform;
        }

        private void FrameworkElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsFaceUp)
                VisualStateManager.GoToState(this, "FaceUp", true);
            else
                VisualStateManager.GoToState(this, "FaceDown", true);
        }

        public ImageSource Front
        {
            get { return (ImageSource)GetValue(FrontProperty); }
            set { SetValue(FrontProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Front.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FrontProperty =
            DependencyProperty.Register("Front", typeof(ImageSource), typeof(CardControl), new PropertyMetadata(null));

        public ImageSource Back
        {
            get { return (ImageSource)GetValue(BackProperty); }
            set { SetValue(BackProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Back.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackProperty =
            DependencyProperty.Register("Back", typeof(ImageSource), typeof(CardControl), new PropertyMetadata(null));

        public bool IsFaceUp
        {
            get { return (bool)GetValue(IsFaceUpProperty); }
            set { SetValue(IsFaceUpProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFaceUp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFaceUpProperty =
            DependencyProperty.Register("IsFaceUp", typeof(bool), typeof(CardControl), new PropertyMetadata(false, IsFaceUpChanged));

        private static void IsFaceUpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as CardControl;
            if ((bool)e.NewValue)
                VisualStateManager.GoToState(me, "FaceUp", true);
            else
                VisualStateManager.GoToState(me, "FaceDown", true);
        }

        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        // Using a DependencyProperty as the backing store for X.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(CardControl), new PropertyMetadata(0.0, XChaged));

        private static void XChaged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as CardControl;
            var story = new Windows.UI.Xaml.Media.Animation.Storyboard();
            var ani = new DoubleAnimation();
            //ani.Duration = TimeSpan.FromSeconds(2);
            ani.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut };
            ani.To = (double)e.NewValue;
            Storyboard.SetTarget(ani, me.transform);
            Storyboard.SetTargetProperty(ani, "TranslateX");
            story.Children.Add(ani);
            story.Begin();
        }

        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Y.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(CardControl), new PropertyMetadata(0.0, YChanged));

        private static void YChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as CardControl;
            var story = new Windows.UI.Xaml.Media.Animation.Storyboard();
            var ani = new DoubleAnimation();
            //ani.Duration = TimeSpan.FromSeconds(2);
            ani.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut };
            ani.To = (double)e.NewValue;
            Storyboard.SetTarget(ani, me.transform);
            Storyboard.SetTargetProperty(ani, "TranslateY");
            story.Children.Add(ani);
            story.Begin();
        }

        public double Rotation
        {
            get { return (double)GetValue(RotationProperty); }
            set { SetValue(RotationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Rotation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RotationProperty =
            DependencyProperty.Register("Rotation", typeof(double), typeof(CardControl), new PropertyMetadata(0.0, RotationChanged));

        private static void RotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as CardControl;

            me.transform.CenterX = 0.5;
            me.transform.CenterY = 0.5;
            var rotationStory = new Windows.UI.Xaml.Media.Animation.Storyboard();
            var ani = new DoubleAnimation();
            //ani.Duration = TimeSpan.FromSeconds(2);
            ani.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut };
            ani.To = (double)e.NewValue;
            Storyboard.SetTarget(ani, me.transform);
            Storyboard.SetTargetProperty(ani, "Rotation");
            rotationStory.Children.Add(ani);
            rotationStory.Begin();

        }
    }
}