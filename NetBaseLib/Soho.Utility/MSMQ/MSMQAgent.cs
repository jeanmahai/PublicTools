using System;
using System.Messaging;
using System.Collections.Generic;

namespace Soho.Utility.MSMQ
{
    public delegate void ProcessMessageHandle(string messageLable, string messageBody);

    public class MSMQAgent
    {
        private string _MQPath = "";
        private MessageQueue _MsgQueue = null;

        public MSMQAgent(string agentName, string busName)
        {
            InitAgent(agentName, busName);
        }

        public MSMQAgent(string agentName)
        {
            InitAgent(agentName, MSMQDefine.DEFAULT_BUSNAME);
        }

        private void InitAgent(string agentName, string busName)
        {
            if (string.IsNullOrEmpty(agentName))
                throw new Exception("MSMQ path cannot be null or empty.");

            Dictionary<string, string> config = ConfigManager.Config;
            if (config == null)
                throw new Exception("Configure file \"MSMQConfig.xml\" cannot be found.");

            string mqPath = ConfigManager.GetItem(agentName, busName);
            if (string.IsNullOrEmpty(mqPath))
                throw new Exception("Cannot find MSMQ path.");

            _MQPath = mqPath;
        }

        public static MSMQAgent GetMSMQAgent(string agentName)
        {
            MSMQAgent agent = null;
            try
            {
                agent = new MSMQAgent(agentName, MSMQDefine.DEFAULT_BUSNAME);
            }
            catch { }
            return agent;
        }

        public static MSMQAgent GetMSMQAgent(string agentName, string busName)
        {
            MSMQAgent agent = null;
            try
            {
                agent = new MSMQAgent(agentName, busName);
            }
            catch { }
            return agent;
        }

        /// <summary>
        /// 获取当前消息队列的实例
        /// </summary>
        /// <returns></returns>
        public MessageQueue GetQueue()
        {
            if (_MsgQueue != null)
                return _MsgQueue;

            if (string.IsNullOrEmpty(_MQPath))
                throw new Exception("Create MessageQueue instance failed:cannot find path.");

            try
            {
                //非独占方式
                MessageQueue result = new MessageQueue(_MQPath, false);
                result.Formatter = new BinaryMessageFormatter();

                _MsgQueue = result;
            }
            catch { _MsgQueue = null; }
            finally { }

            return _MsgQueue;
        }

        /// <summary>
        /// 非事物机制发送消息
        /// </summary>
        /// <param name="label"></param>
        /// <param name="body"></param>
        public void SendMessage(string label, string body)
        {
            SendMessage(label, body, false);
        }

        /// <summary>
        /// 指定是否使用事物机制来发送消息
        /// </summary>
        /// <param name="label"></param>
        /// <param name="body"></param>
        /// <param name="transactional"></param>
        public void SendMessage(string label, string body, bool transactional)
        {
            if (!transactional)
            {
                SendMessageToMSMQ(label, body, null);
            }
            else
            {
                MessageQueueTransaction myTransaction = new MessageQueueTransaction();
                myTransaction.Begin();
                SendMessageToMSMQ(label, body, myTransaction);
                myTransaction.Commit();
            }
        }

        private bool SendMessageToMSMQ(string label, string body, MessageQueueTransaction ms)
        {
            bool result = false;

            if (string.IsNullOrEmpty(label) || string.IsNullOrEmpty(body))
                return result;

            try
            {
                MessageQueue queue = GetQueue();

                if (queue == null)
                    return result;

                if (ms != null)
                    queue.Send(body, label, ms);
                else
                    queue.Send(body, label);

                result = true;
            }
            catch { result = false; }
            finally { }

            return result;
        }

        /// <summary>
        /// 创建消息队列
        /// </summary>
        /// <returns></returns>
        public bool Create()
        {
            bool result = false;

            if (string.IsNullOrEmpty(_MQPath))
                return result;

            try
            {
                if (!MessageQueue.Exists(_MQPath))
                {
                    System.Messaging.MessageQueue.Create(_MQPath);
                }

                result = true;
            }
            catch { }
            finally { }

            return result;
        }

        /// <summary>
        /// 删除消息队列
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            bool result = false;

            try
            {
                System.Messaging.MessageQueue.Delete(_MQPath);
                result = true;
            }
            catch { result = false; }

            return result;
        }

        /// <summary>
        /// 读取消息队列上的全部消息
        /// </summary>
        /// <returns></returns>
        public System.Collections.ArrayList GetAllMessages()
        {
            System.Collections.ArrayList result = null;

            try
            {
                MessageQueue queue = GetQueue();

                if (queue == null)
                    return result;

                queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                System.Messaging.Message[] allMessages = queue.GetAllMessages();
                queue.Close();

                if (allMessages == null || allMessages.Length < 1)
                    return result;

                result = new System.Collections.ArrayList();

                foreach (System.Messaging.Message msg in allMessages)
                {
                    result.Add(msg.Body.ToString());
                }
            }
            catch
            {
                result = null;
            }
            finally { }

            return result;
        }

        /// <summary>
        /// 删除消息队列上的全部消息
        /// </summary>
        public void Purge()
        {
            try
            {
                MessageQueue queue = GetQueue();

                if (queue != null)
                    queue.Purge();
            }
            catch
            {

            }
            finally { }
        }

        public void Dispose()
        {
            try
            {
                if (_MsgQueue != null)
                {
                    _MsgQueue.Close();
                    _MsgQueue.Dispose();
                }
            }
            catch { }
            finally { _MsgQueue = null; }
        }

    }
}
