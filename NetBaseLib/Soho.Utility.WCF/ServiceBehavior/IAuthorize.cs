using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soho.Utility.WCF
{
    public interface IAuthorize
    {
        bool Check(string userIdentity, string methodName, string urlTemplate, string url);
    }
}
