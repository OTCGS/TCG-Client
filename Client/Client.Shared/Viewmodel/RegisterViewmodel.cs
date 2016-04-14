using Client.Common;
using Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Storage.Streams;
using Windows.UI.Xaml;



namespace Client.Viewmodel
{
    public class RegisterViewmodel : DependencyObject
    {


        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLoading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(RegisterViewmodel), new PropertyMetadata(false));

        public string UserName
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register(nameof(UserName), typeof(string), typeof(RegisterViewmodel), new PropertyMetadata(""));



        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Password.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(nameof(Password), typeof(string), typeof(RegisterViewmodel), new PropertyMetadata(""));




        public string PasswordRepeat
        {
            get { return (string)GetValue(PasswordRepeatProperty); }
            set { SetValue(PasswordRepeatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PasswordRepeat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PasswordRepeatProperty =
            DependencyProperty.Register(nameof(PasswordRepeat), typeof(string), typeof(RegisterViewmodel), new PropertyMetadata(""));




        public byte[] Image
        {
            get { return (byte[])GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(nameof(Image), typeof(byte[]), typeof(RegisterViewmodel), new PropertyMetadata(null));




        public RelayCommand SelectImageCommand
        {
            get
            {
                return new RelayCommand(SelectImage);
            }
        }
        public RelayCommand RegisterCommand
        {
            get
            {
                return new RelayCommand(Register);
            }
        }




        private async void SelectImage()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            var erg = await picker.PickSingleFileAsync();
            if (erg == null)
                return;
            try
            {

                IsLoading = true;
                var stream = await erg.OpenReadAsync();
                IBuffer readed;
                List<byte> data = new List<byte>();
                do
                {
                    var b = new byte[1024].AsBuffer();

                    readed = await stream.ReadAsync(b, b.Length, Windows.Storage.Streams.InputStreamOptions.ReadAhead);
                    if (readed.Length > 0)
                        data.AddRange(readed.ToArray());
                } while (readed.Length > 0);
                var imageData = data.ToArray();
                Image = imageData;
            }
            finally
            {
                IsLoading = true;
            }
        }

        private async void Register()
        {
            Windows.UI.Popups.MessageDialog error = null;

            if (Password != PasswordRepeat)
                error = new Windows.UI.Popups.MessageDialog("Die beiden Passwörter müssen identisch sein.");
            else
            {

                try
                {
                    IsLoading = true;
                    await UserDataViewmodel.Instance.CreateUserAccount(UserName, Password, Image);
                }
                catch (Exception e)
                {
                    String text = e.ToString() + "\n" + e.Message;

                    error = new Windows.UI.Popups.MessageDialog("Leider ist ein unbekannter Fehler aufgetreten.\n" + text, "Fehler");
                }
                finally
                {
                    IsLoading = false;
                }
            }
            if (error != null)
                await error.ShowAsync();
            else
                App.RootFrame.Navigate(typeof(HubPage));
        }
    }
}