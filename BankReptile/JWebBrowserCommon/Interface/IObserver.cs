using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JWebBrowserCommon.Interface
{
    public interface IObserver
    {
        List<Trigger> TiggerList
        {
            get;            
        }
    }
}
