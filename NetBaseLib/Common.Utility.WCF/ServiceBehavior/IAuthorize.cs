using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utility.WCF
{
    public interface IAuthorize
    {
        bool Check(string userIdentity, string methodName, string urlTemplate, string url);
    }
}
