using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Client.Common.Converters
{
    class BooleanToVisibilityConverter : ConverterWithDefinedTargetType
    {
        /// <summary>
        /// If set to True, conversion is reversed: True will become Collapsed.
        /// </summary>
        public bool FalseIsVisible { get; set; }
        public Visibility DefaultVisibility { get; set; } = Visibility.Collapsed;

        public override Type ReturnType { get; } = typeof(Visibility);
        public override IEnumerable<Type> InputTypes { get; } = new Type[] { typeof(bool) };

        protected override object InternalCovert(object value, object parameter, string language)
        {
            var b = (bool)value;
            if (this.FalseIsVisible)
                b = !b;

            return b ? Visibility.Visible : Visibility.Collapsed;
        }




    }
}
