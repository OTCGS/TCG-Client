using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    //
    // Summary:
    //     Stellt die Methode dar, die ein Ereignis behandelt, wenn das Ereignisdaten bereitstellt.
    //
    // Type parameters:
    //   TEventArgs:
    //     Der Typ der vom Ereignis generierten Ereignisdaten.
    public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e);
}
