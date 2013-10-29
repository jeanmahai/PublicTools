namespace ExWebServer.WebServer.HttpLib
{
    public enum ConnectionMode
    {
        /// <summary>
        /// Persist the connection after the response has been sent to the client.
        /// </summary>
        KeepAlive,
        /// <summary>
        /// Disconnect the client after the response has been sent.
        /// </summary>
        Close
    }
}
