using System;
using System.Collections.Generic;
using System.Threading;

namespace ITXCM
{
    /// <summary>
    /// MessageDistributer
    /// 消息分发器
    /// </summary>
    public class MessageDistributer : MessageDistributer<object>
    {
    }

    /// <summary>
    /// 消息分发器
    /// MessageDistributer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageDistributer<T> : Singleton<MessageDistributer<T>>
    {
        class MessageArgs
        {
            public T sender;
            public SkillBridge.Message.NetMessage message;
        }
        private Queue<MessageArgs> messageQueue = new Queue<MessageArgs>();

        public delegate void MessageHandler<Tm>(T sender, Tm message);
        private Dictionary<string, System.Delegate> messageHandlers = new Dictionary<string, System.Delegate>();

        private bool Running = false;
        private AutoResetEvent threadEvent = new AutoResetEvent(true);

        public int ThreadCount = 0;
        public int ActiveThreadCount = 0;

        public bool ThrowException = false;

        public MessageDistributer()
        {
        }

        public void Subscribe<Tm>(MessageHandler<Tm> messageHandler)
        {
            string type = typeof(Tm).Name;
            if (!messageHandlers.ContainsKey(type))
            {
                messageHandlers[type] = null;
            }
            messageHandlers[type] = (MessageHandler<Tm>)messageHandlers[type] + messageHandler;
        }
        public void Unsubscribe<Tm>(MessageHandler<Tm> messageHandler)
        {
            string type = typeof(Tm).Name;
            if (!messageHandlers.ContainsKey(type))
            {
                messageHandlers[type] = null;
            }
            messageHandlers[type] = (MessageHandler<Tm>)messageHandlers[type] - messageHandler;
        }

        public void RaiseEvent<Tm>(T sender, Tm msg)
        {
            string key = msg.GetType().Name;
            if (messageHandlers.ContainsKey(key))
            {
                MessageHandler<Tm> handler = (MessageHandler<Tm>)messageHandlers[key];
                if (handler != null)
                {
                    try
                    {
                        handler(sender, msg);
                    }
                    catch (System.Exception ex)
                    {
                        Log.ErrorFormat("Message handler exception:{0}, {1}, {2}, {3}", ex.InnerException, ex.Message, ex.Source, ex.StackTrace);
                        if (ThrowException)
                            throw ex;
                    }
                }
                else
                {
                    Log.Warning("No handler subscribed for {0}" + msg.ToString());
                }
            }
        }

        public void ReceiveMessage(T sender, SkillBridge.Message.NetMessage message)
        {
            this.messageQueue.Enqueue(new MessageArgs() { sender = sender, message = message });
            threadEvent.Set();
        }

        public void Clear()
        {
            this.messageQueue.Clear();
        }

        /// <summary>
        /// 一次性分发队列中的所有消息
        /// </summary>
        public void Distribute()
        {
            if (this.messageQueue.Count == 0)
            {
                return;
            }

            while (this.messageQueue.Count > 0)
            {
                MessageArgs package = this.messageQueue.Dequeue();
                if (package.message.Request != null)
                    MessageDispatch<T>.Instance.Dispatch(package.sender, package.message.Request);
                if (package.message.Response != null)
                    MessageDispatch<T>.Instance.Dispatch(package.sender, package.message.Response);
            }
        }

        /// <summary>
        /// 启动消息处理器
        /// [多线程模式]
        /// </summary>
        /// <param name="ThreadNum">工作线程数</param>
        public void Start(int ThreadNum)
        {
            this.ThreadCount = ThreadNum;
            if (this.ThreadCount < 1) this.ThreadCount = 1;
            if (this.ThreadCount > 1000) this.ThreadCount = 1000;
            Running = true;
            for (int i = 0; i < this.ThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(MessageDistribute));
            }
            while (ActiveThreadCount < this.ThreadCount)
            {
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 停止消息处理器
        /// [多线程模式]
        /// </summary>
        public void Stop()
        {
            Running = false;
            this.messageQueue.Clear();
            while (ActiveThreadCount > 0)
            {
                threadEvent.Set();
            }
            Thread.Sleep(100);
        }

        /// <summary>
        /// 消息处理线程
        /// [多线程模式]
        /// </summary>
        /// <param name="stateInfo"></param>
        private void MessageDistribute(Object stateInfo)
        {
            Log.Warning("MessageDistribute thread start");
            try
            {
                ActiveThreadCount = Interlocked.Increment(ref ActiveThreadCount);
                while (Running)
                {
                    if (this.messageQueue.Count == 0)
                    {
                        threadEvent.WaitOne();
                        //Log.WarningFormat("[{0}]MessageDistribute Thread[{1}] Continue:", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
                        continue;
                    }
                    MessageArgs package = this.messageQueue.Dequeue();
                    if (package.message.Request != null)
                        MessageDispatch<T>.Instance.Dispatch(package.sender, package.message.Request);
                    if (package.message.Response != null)
                        MessageDispatch<T>.Instance.Dispatch(package.sender, package.message.Response);
                }
            }
            catch
            {
            }
            finally
            {
                ActiveThreadCount = Interlocked.Decrement(ref ActiveThreadCount);
                Log.Warning("MessageDistribute thread end");
            }
        }
    }
}