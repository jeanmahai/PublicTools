namespace ExWebServer.SocketBase.Client
{
    public class UserState
    {
        public int nUID { get; set; }
        public string sName { get; set; }

        public UserState()
        {
            this.nUID = 0;
            this.sName = "";
        }

        public UserState(int uid, string name)
        {
            this.nUID = uid > 0 ? uid : 0;
            this.sName = !string.IsNullOrEmpty(name) ? name.Trim() : "";
        }
    }
}
