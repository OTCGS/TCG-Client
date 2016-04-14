using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Benutzersteuerelement" ist unter http://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace Client.Store.Dialogs
{
    public sealed partial class CreateAccountDialog : UserControl
    {
        public Viewmodel.Account.CreateAccountViewmodel DefaultViewModel
        {
            get { return this.DataContext as Viewmodel.Account.CreateAccountViewmodel; }
        }

        public CreateAccountDialog()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            var erg = await picker.PickSingleFileAsync();
            if (erg == null)
                return;
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
            this.DefaultViewModel.Image = imageData;
        }
    }
}