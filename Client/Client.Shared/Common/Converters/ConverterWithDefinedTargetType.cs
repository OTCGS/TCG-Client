using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Client.Common.Converters
{
    public abstract class ConverterWithDefinedTargetType : Windows.UI.Xaml.Data.IValueConverter
    {

        public abstract Type ReturnType { get; }
        public abstract IEnumerable<Type> InputTypes { get; }


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!targetType.GetTypeInfo().IsAssignableFrom(ReturnType.GetTypeInfo()))
            {
                Logger.Failure($"TypFehler in Converter ({GetType().Name}) Targettyp war {targetType}.");
                return null;
            }
            if (!InputTypes.Any(x => value == x || (value != null && x != null && x.GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo()))))
                return null;
            return InternalCovert(value, parameter,language);
        }

        protected abstract object InternalCovert(object value, object parameter, string language);
        public virtual object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
