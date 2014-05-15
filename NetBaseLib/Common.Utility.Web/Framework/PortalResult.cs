namespace Common.Utility.Web.Framework
{
    public class PortalResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public int Code { get; set; }
        internal string Cookie { get; set; }
    }
}
