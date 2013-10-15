using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utility.Web
{
    internal class MobileCookiePersister : ICookiePersist
    {
        #region ICookiePersist Members

        public void Save(string cookieName, string cookieValue, Dictionary<string, string> parameters)
        {
            throw new NotImplementedException();
        }

        public string Get(string cookieName, Dictionary<string, string> parameters)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
