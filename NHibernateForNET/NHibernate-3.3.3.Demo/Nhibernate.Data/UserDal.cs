using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using Nhibernate.Domain;

namespace Nhibernate.Data
{
    public class UserDal
    {
        private NHibernateHelper nhibernateHelper = new NHibernateHelper();

        protected ISession Session { get; set; }

        public UserDal() {
            this.Session = nhibernateHelper.GetSession();
        }

        public UserDal(ISession session)
        {
            this.Session = session;
        }

        public void CreateCustomer(User user)
        {
            Session.Save(user);
            Session.Flush();
        }

        public User GetCustomerById(string name)
        {
            return Session.Get<User>(name);
        }

        public IList<User> GetCunstomers()
        {
            IList<User> list = null;
            list = Session.QueryOver<User>().List();
            return list;
        }
    }
}
