using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Client.Common.Converters
{
    public class NullToFalseConverter : ConverterWithDefinedTargetType
    {
        public override IEnumerable<Type> InputTypes { get; } = new Type[] { null, typeof(object) };

        public override Type ReturnType { get; } = typeof(bool);

        protected override object InternalCovert(object value, object parameter, string language)
        {
            return value != null;
        }


    }
}