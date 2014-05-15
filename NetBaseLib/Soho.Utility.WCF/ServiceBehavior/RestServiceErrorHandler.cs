using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Net;
using System.ServiceModel.Security;
using System.Web;
using System.Threading;
using System.ServiceModel.Web;
using System.Xml;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Configuration;

namespace Soho.Utility.WCF
{
    public class RestServiceErrorHandler : IErrorHandler
    {
        private Type[] m_BizExceptionTypeList;
        private IExceptionHandle m_ExceptionHandler;

        public RestServiceErrorHandler(Type[] typeList, Type handleType)
        {
            if (typeList != null && typeList.Length > 0)
            {
                foreach (var type in typeList)
                {
                    if (type != null && !type.IsSubclassOf(typeof(Exception)))
                    {
                        throw new ArgumentException("The type must derive from 'System.Exception'.", "type");
                    }
                }
            }
            m_BizExceptionTypeList = typeList;
            if (handleType != null)
            {
                if (!typeof(IExceptionHandle).IsAssignableFrom(handleType))
                {
                    throw new ArgumentException("The type must implement interface 'Nesoft.Utility.WCF.IExceptionHandle'.", "handleType");
                }
                m_ExceptionHandler = (IExceptionHandle)Activator.CreateInstance(handleType);
            }
            if (m_ExceptionHandler == null)
            {
                m_ExceptionHandler = new ExceptionHandler();
            }
        }

        private bool CheckIsBizException(Type type)
        {
            if (m_BizExceptionTypeList != null && m_BizExceptionTypeList.Length > 0)
            {
                foreach (var bizType in m_BizExceptionTypeList)
                {
                    if (bizType != null && bizType.IsAssignableFrom(type))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HandleError(Exception error)
        {
            if (m_ExceptionHandler != null)
            {
                if (error != null && CheckIsBizException(error.GetType())) // BizExcpetion不需要记录日志
                {
                    return true;
                }
                object requestParams;
                OperationContext.Current.OutgoingMessageProperties.TryGetValue("_RequestParams_", out requestParams);
                m_ExceptionHandler.Handle(error, requestParams as object[]);
            }
            return true;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            HttpStatusCode statusCode;
            if (error is SecurityAccessDeniedException)
            {
                statusCode = HttpStatusCode.Unauthorized;
            }
            else if (error is ServerTooBusyException)
            {
                statusCode = HttpStatusCode.ServiceUnavailable;
            }
            else if (CheckIsBizException(error.GetType()))
            {
                statusCode = HttpStatusCode.OK;
            }
            else
            {
                statusCode = HttpStatusCode.InternalServerError;
            }

            RestServiceError errorData = new RestServiceError()
            {
                StatusCode = (int)statusCode,
                StatusDescription = HttpWorkerRequest.GetStatusDescription((int)statusCode),
                Faults = new List<Error>()
            };

            if (CheckIsBizException(error.GetType()))
            {
                errorData.Faults.Add(new Error()
                {
                    ErrorCode = "11111",
                    ErrorDescription = error.Message
                });
            }
            else
            {
                string show = ConfigurationManager.AppSettings["ShowErrorDetail"];
                bool showDetail = false;
                bool tmp;
                if (!string.IsNullOrWhiteSpace(show) && bool.TryParse(show, out tmp))
                {
                    showDetail = tmp;
                }
                errorData.Faults.Add(new Error()
                {
                    ErrorCode = "00000",
                    ErrorDescription = showDetail ? error.ToString() : "SYSTEM EXCEPTION"
                });
            }

            if (version == MessageVersion.None)
            {
                WebMessageFormat messageFormat = WebOperationContext.Current.OutgoingResponse.Format ?? WebMessageFormat.Xml;
                WebContentFormat contentFormat = WebContentFormat.Xml;
                string contentType = "text/xml";

                if (messageFormat == WebMessageFormat.Json)
                {
                    contentFormat = WebContentFormat.Json;
                    contentType = "application/json";
                }

                WebBodyFormatMessageProperty bodyFormat = new WebBodyFormatMessageProperty(contentFormat);

                HttpResponseMessageProperty responseMessage = new HttpResponseMessageProperty();
                responseMessage.StatusCode = HttpStatusCode.OK;
                responseMessage.StatusDescription = HttpWorkerRequest.GetStatusDescription((int)responseMessage.StatusCode);
                responseMessage.Headers[HttpResponseHeader.ContentType] = contentType;
                responseMessage.Headers["X-HTTP-StatusCode-Override"] = "500";

                fault = Message.CreateMessage(MessageVersion.None, null, new RestServiceErrorWriter() { Error = errorData, Format = contentFormat });
                fault.Properties[WebBodyFormatMessageProperty.Name] = bodyFormat;
                fault.Properties[HttpResponseMessageProperty.Name] = responseMessage;
            }
        }

        class RestServiceErrorWriter : BodyWriter
        {
            public RestServiceErrorWriter()
                : base(true)
            { }

            public RestServiceError Error { get; set; }

            public WebContentFormat Format { get; set; }

            protected override BodyWriter OnCreateBufferedCopy(int maxBufferSize)
            {
                return base.OnCreateBufferedCopy(maxBufferSize);
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                if (Format == WebContentFormat.Json)
                {
                    new DataContractJsonSerializer(typeof(RestServiceError)).WriteObject(writer, Error);
                }
                else
                {
                    new DataContractSerializer(typeof(RestServiceError)).WriteObject(writer, Error);
                }
            }
        }
    }
}
