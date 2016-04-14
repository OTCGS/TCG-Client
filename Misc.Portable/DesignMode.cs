using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class DesignMode
    {
        public static bool Enabled
        {
            get
            {
                var rt = Type.GetType("Windows.ApplicationModel.DesignMode, Windows, ContentType=WindowsRuntime", false);
                if (rt != null)
                    return (bool)rt.GetRuntimeProperty("DesignModeEnabled").GetValue(null);
                //return System.ComponentModel.DesignerProperties.GetIsInDesignMode(new FrameworkElement());

                var desktop = Type.GetType("System.ComponentModel.DesignerProperties, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", false);

                if (desktop != null)
                {
                    var dpType = Type.GetType("System.Windows.DependencyObject, WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                    var method = desktop.GetRuntimeMethod("GetIsInDesignMode", new Type[] { dpType });
                    var frameworkType = Type.GetType("System.Windows.FrameworkElement, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                    var finstance = Activator.CreateInstance(frameworkType);
                    return (bool)method.Invoke(null, new object[] { finstance });
                }
                throw new NotSupportedException();
            }
        }
    }
}