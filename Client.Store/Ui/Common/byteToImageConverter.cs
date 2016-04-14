using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Client.Store.Common
{
    public class ByteToImageConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is byte[]))
                return null;
            if (targetType == typeof(ImageSource))
            {
                try
                {
                    var b = value as byte[];
                    if (b.Length == 0)
                        return null;
                    BitmapImage bt = new BitmapImage();

                    SetSource(bt, b);
                    return bt;
                }
                catch (Exception)
                {
                    System.Diagnostics.Debugger.Break();
                    return null;
                }
            }

            return null;
        }

        private static async void SetSource(BitmapImage bt, byte[] bArray)
        {
            try
            {
                var mem = new MemoryStream(bArray);
                var ras = mem.AsRandomAccessStream();
                await bt.SetSourceAsync(ras);
                mem.Dispose();
            }
            catch (Exception)
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}