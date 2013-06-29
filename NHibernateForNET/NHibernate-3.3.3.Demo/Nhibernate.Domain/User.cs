using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nhibernate.Domain
{
    [Serializable]
    public class User
    {
        public virtual int ID { get; set; }
        public virtual string Name { get; set; }
        public virtual string Sex { get; set; }
    }
}
