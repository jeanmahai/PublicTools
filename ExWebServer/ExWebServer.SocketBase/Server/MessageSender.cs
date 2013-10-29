using System;
using System.Threading;
using System.Net.Sockets;
using System.Collections;
using System.ComponentModel;

using ExWebServer.SocketBase.Client;
using ExWebServer.SocketBase.Protocals;

namespace ExWebServer.SocketBase.Server
{
    public class MessageSender
    {
        private Queue _OutingMessage = null;

        public event ShutDownClientEventHandler ShutDownClient;

        private void Init()
        {
        }

        public MessageSender()
        {
            Init();
        }

        public bool AppendToOutMessageQueue(QueuedOutMessage msg)
        {
            return true;
        }

        private void StartMessageDeliver(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                Thread.Sleep(15);
                try
                {
                    if (_OutingMessage != null && _OutingMessage.Count > 0)
                    {
                        lock (_OutingMessage.SyncRoot)
                        {
                            if (_OutingMessage.Count > 0)
                            {
                                QueuedOutMessage msg = _OutingMessage.Dequeue() as QueuedOutMessage;
                                SendAsc(msg);
                            }
                        }
                    }
                }
                catch { }
                finally { }
            }
        }

        public void SendAsc(QueuedOutMessage msg)
        {
            try
            {
                if (msg == null || msg.Client == null || !msg.Client.Socket.Connected || msg.Command == null)
                    return;

                ClientManager clmngr = msg.Client;
                ISocketMessage message = msg.Command;

                byte[] buf = message.GetBytes();
                msg.Length = (uint)buf.Length;
                msg.ShutdownClientAfterSend = true;
                clmngr.Socket.BeginSend(buf, 0, buf.Length, SocketFlags.None, new AsyncCallback(SendAscyCallback), msg);
            }
            catch
            {
            }
            finally { }
        }

        public void SendAscyCallback(IAsyncResult ar)
        {
            QueuedOutMessage msg = null;
            try
            {
                msg = (QueuedOutMessage)ar.AsyncState;
                if (msg == null || msg.Client == null)
                    return;

                uint bytesSent = (uint)msg.Client.Socket.EndSend(ar);
                if (msg.ShutdownClientAfterSend && this.ShutDownClient != null)
                {
                    ShutDownClient(this, new ClientShutDownEventArgs(msg.Client));
                }
                //发送不成功
                if (bytesSent != msg.Length)
                    AppendToOutMessageQueue(msg);
            }
            catch (Exception ex)
            {
                if (msg != null)
                {
                    AppendToOutMessageQueue(msg);
                }
                if (this.ShutDownClient != null)
                {
                    ShutDownClient(this, new ClientShutDownEventArgs(msg.Client));
                }
                Console.WriteLine(string.Format("SendAscyCallback Error:{0}", ex.Message));
            }
        }
    }
}
