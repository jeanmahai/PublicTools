using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nhibernate.Data;
using Nhibernate.Domain;

namespace Nhibernate.Web
{
    public partial class WebForm1:System.Web.UI.Page
    {
        protected void Page_Load(object sender,EventArgs e)
        {
            var dal = new UserDal();
            dal.CreateCustomer(new User()
                               {
                                   Name = "name",
                                   Sex = "maile",
                                   ID = 1
                               });
            var data = dal.GetCunstomers();
            Response.Write(data.Count);
        }
    }
}