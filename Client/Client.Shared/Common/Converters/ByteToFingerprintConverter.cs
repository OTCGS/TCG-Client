using System.Reflection;
using Security;
using Security.Interfaces;
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
    public class PublicKeyToFingerprintConverter : ConverterWithDefinedTargetType
    {
        public override IEnumerable<Type> InputTypes { get; } = new Type[] { typeof(IPublicKeyData) };

        public override Type ReturnType { get; } = typeof(string);

        protected override object InternalCovert(object value, object parameter, string language)
        {
            var pk = value as IPublicKeyData;
            return pk.FingerPrint();
        }



    }
}