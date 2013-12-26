using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utility.WCF
{
    public class RestWebServiceStartup : IStartup
    {
        private string m_CustomBizExceptionTypeName;
        private string m_ExceptionHandlerTypeName;
        private string m_ConverterTypeName;

        public RestWebServiceStartup() : this(null)
        {

        }

        public RestWebServiceStartup(string customBizExceptionTypeName)
            : this(customBizExceptionTypeName, null)
        {

        }

        public RestWebServiceStartup(string customBizExceptionTypeName, string exceptionHandlerTypeName)
            : this(customBizExceptionTypeName, exceptionHandlerTypeName, null)
        {

        }

        public RestWebServiceStartup(string customBizExceptionTypeName, string exceptionHandlerTypeName, string converterTypeName)
        {
            m_CustomBizExceptionTypeName = customBizExceptionTypeName;
            m_ExceptionHandlerTypeName = exceptionHandlerTypeName;
            m_ConverterTypeName = converterTypeName;
        }

        public void Start()
        {
            RestWebServiceHost.Open(m_CustomBizExceptionTypeName, m_ExceptionHandlerTypeName, m_ConverterTypeName);
        }
    }
}
