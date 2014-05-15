using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Messaging;

namespace Soho.Utility.MSMQ
{
    public class MSMQReceiver
    {
        public event ProcessMessageHandle OnMessageArrivedHandle = null;

        private MSMQAgent _MQAgent = null;


        public MSMQReceiver(string agentName)
        {
            InitReceiver(agentName, MSMQDefine.DEFAULT_BUSNAME);
        }

        public MSMQReceiver(string agentName, string busName)
        {
            InitReceiver(agentName, busName);
        }

        protected void InitReceiver(string agentName, string busName)
        {
            _MQAgent = MSMQAgent.GetMSMQAgent(agentName, busName);

            if (_MQAgent == null || _MQAgent.GetQueue() == null)
                throw new Exception("Init receiver failed.");

            if (_MQAgent.GetQueue() == null)
                throw new Exception("Init receiver's queue failed.");
        }

        public void RegisterMessageHandle(ProcessMessageHandle handle)
        {
            this.OnMessageArrivedHandle += handle;
        }

        public bool StartReceiver()
        {
            //mq.CreateMQ();
            MessageQueue myQueue = null;
            bool result = false;
            try
            {
                myQueue = _MQAgent.GetQueue();
                if (myQueue == null)
                    return result;
                //异步
                myQueue.PeekCompleted += new PeekCompletedEventHandler(PeekCompleted);
                myQueue.BeginPeek();
                result = true;
            }
            catch { }
            finally { }
            return result;

        }
        public void DisposeMessage()
        {
            _MQAgent.Dispose();
        }

        protected void PeekCompleted(object sender, PeekCompletedEventArgs e)
        {
            System.Messaging.Message msg = null;
            MessageQueue mq = null;

            try
            {
                mq = (MessageQueue)sender;
                mq.Formatter = new BinaryMessageFormatter();
                System.Messaging.Message m = mq.EndPeek(e.AsyncResult);
                mq.ReceiveById(m.Id);
                if (OnMessageArrivedHandle != null)
                {
                    OnMessageArrivedHandle(m.Label, m.Body.ToString());
                }
            }
            catch (Exception ex) { string err = ex.Message; }
            finally
            {
                if (msg != null)
                {
                    try { msg.Dispose(); }
                    catch { }
                    finally { msg = null; }
                }
                //接收下一次事件
                if (mq != null)
                    mq.BeginPeek();
            }
        }
    }
}
