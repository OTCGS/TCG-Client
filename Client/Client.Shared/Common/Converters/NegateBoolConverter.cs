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
    public class NegateBoolConverter : ConverterWithDefinedTargetType
    {
        public override IEnumerable<Type> InputTypes { get; } = new Type[] { typeof(bool) };

        public override Type ReturnType { get; } = typeof(bool);

        protected override object InternalCovert(object value, object parameter, string language)
        {
            return !((bool)value);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(bool) && value is bool)
                return !((bool)value);

            return null;
        }


    }
}