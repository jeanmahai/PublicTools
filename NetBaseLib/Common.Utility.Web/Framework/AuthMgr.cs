using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Common.Utility.Web.Framework
{
    public static class AuthMgr
    {
        private static IAuth _Auth;

        static AuthMgr()
        {
            AuthConfigurationSection section =
                (AuthConfigurationSection)ConfigurationManager.GetSection("auth");
            if (string.IsNullOrEmpty(section.Default))
            {
                throw new ConfigurationErrorsException("Please config default auth name.");
            }
            if (section.Auths == null || section.Auths.Count == 0)
            {
                throw new ConfigurationErrorsException("Please config at least one auth type.");
            }
            bool find = false;
            foreach (AuthItem auth in section.Auths)
            {
                if (auth.Name.Trim().ToLower() == section.Default.ToLower())
                {
                    Type authType = Type.GetType(auth.Type);
                    _Auth = Activator.CreateInstance(authType) as IAuth;
                    find = true;
                }
            }
            if (!find)
            {
                throw new ConfigurationErrorsException("Can't find default auth, Please check default auth name.");
            }
        }

        public static bool ValidateLogin()
        {
            return _Auth.ValidateLogin();
        }

        public static bool ValidateAuth(string controller, string action)
        {
            return _Auth.ValidateAuth(controller, action);
        }
    }
}
