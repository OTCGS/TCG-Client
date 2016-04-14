using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Client.Common.Converters
{
    class ChainConverter : List<ConverterWithDefinedTargetType>, IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            object erg = value;
            for (int i = 0; i < Count; i++)
            {
                var current = this[i];
                var next = (i + 1 < Count) ? this[i + 1] : null;
                Type inputType;
                if (next != null)
                    inputType = next.InputTypes.Where(x=> x!= null).FirstOrDefault(x => x.GetTypeInfo().IsAssignableFrom(current.ReturnType.GetTypeInfo()));
                else
                    inputType = targetType;
                erg = current.Convert(erg, inputType, parameter, language);
            }
            return erg;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }
}
