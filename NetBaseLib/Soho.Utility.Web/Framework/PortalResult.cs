namespace Soho.Utility.Web.Framework
{
    public class PortalResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public int Code { get; set; }
    }

    public class MobilePortalResult : PortalResult
    {
        public MobilePortalResult(PortalResult result)
        {
            this.Success = result.Success;
            this.Message = result.Message;
            this.Data = result.Data;
            this.Code = result.Code;
        }

        public string Cookie { get; set; }
    }
}
