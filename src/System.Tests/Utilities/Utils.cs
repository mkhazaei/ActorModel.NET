using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Tests.Utilities
{
    public static class Utils
    {
        public static TObject Out<TObject>(TObject velue, out TObject @out)
        {
            @out = velue;
            return velue;
        }
    }
}
