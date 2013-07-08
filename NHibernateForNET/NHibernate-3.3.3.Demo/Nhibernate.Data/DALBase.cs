using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Cfg;

namespace Nhibernate.Data
{
    public class DALBase
    {
        private readonly ISessionFactory m_SessionFactory;

        public ISession Session { get { return m_SessionFactory.OpenSession(); } }

        public DALBase()
        {
            m_SessionFactory = GetSessionFactory();
        }

        private ISessionFactory GetSessionFactory()
        {
            return (new Configuration()).Configure().BuildSessionFactory();
        }

        //private ISession GetSession()
        //{
        //    return m_SessionFactory.OpenSession();
        //}
    }
}
