using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Client.Common.Converters
{
    public class ByteToImageConverter : ConverterWithDefinedTargetType
    {

        private static readonly IDictionary<byte[], BitmapImage> lookup = new Common.WeakDictionary<byte[], BitmapImage>();

        public override Type ReturnType { get; } = typeof(ImageSource);

        public override IEnumerable<Type> InputTypes { get; } = new Type[] { typeof(byte[]) };

        protected override object InternalCovert(object value, object parameter, string language)
        {
            try
            {
                var b = value as byte[];
                if (b.Length == 0)
                    return null;

                BitmapImage bt = null;
                if (lookup.ContainsKey(b))
                    bt = lookup[b];
                if (bt == null)
                {
                    bt = new BitmapImage();
                    lookup[b] = bt;
                    SetSource(bt, b);
                }

                return bt;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                return null;
            }
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
            catch (Exception e)
            {
                Logger.LogException(e);
            }
        }



    }
}