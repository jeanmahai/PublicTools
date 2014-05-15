using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soho.Utility
{
    public enum MailSendType
    {
        Smtp,
        Queue,
        RestfulService,
        SoapService
    }
}
