using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Linq.Expressions;

namespace Soho.Utility
{
    public class Map<T> where T : class, new()
    {
        // -- Return -------------------------------------
        public static ReturnMap<T> Add(Expression<Func<T, object>> expression, string columName)
        {
            return new ReturnMap<T>(expression, columName);
        }

        public static ReturnMap<T> Add(Expression<Func<T, object>> expression)
        {
            return new ReturnMap<T>(expression, null);
        }

        // -- Input --------------------------------------
        
        public static InputMap<T> Add(string parameterName, Expression<Func<T, object>> expression)
        {
            return new InputMap<T>(parameterName, expression);
        }
    }
}
