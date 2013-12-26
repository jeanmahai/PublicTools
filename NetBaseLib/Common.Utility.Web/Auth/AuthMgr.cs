using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Common.Utility.Web
{
    public static class AuthMgr
    {
        private static IAuth m_auth;

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
                    m_auth = Activator.CreateInstance(authType) as IAuth;
                    find = true;
                }
            }
            if (!find)
            {
                throw new ConfigurationErrorsException("Can't find default auth, Please check default auth name.");
            }
        }

        public static bool Login(string userName, string pwd, string verifyCode)
        {
            return m_auth.Login(userName, pwd, verifyCode);
        }

        public static bool ValidateAuth()
        {
            return m_auth.ValidateAuth();
        }
    }
}
