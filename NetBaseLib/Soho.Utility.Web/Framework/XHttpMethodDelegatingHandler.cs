using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Soho.Utility.Web.Framework
{
    public class XHttpMethodDelegatingHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //fixed cross domain
            if (request.Method == System.Net.Http.HttpMethod.Options)
            {
                return Task<HttpResponseMessage>.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    return response;
                });
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
