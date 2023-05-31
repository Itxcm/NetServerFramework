using System;
using System.Net;
using System.Net.Sockets;
using ITXCM;

namespace GameServer.Network
{
    /// <summary>
    /// A connection to our server.
    /// </summary>
    public class NetConnection<T> where T : INetSession
    {
        /// <summary>
        /// Represents a callback used to inform a listener that a ServerConnection has received data.
        /// </summary>
        /// <param name="sender">The sender of the callback.</param>
        /// <param name="e">The DataEventArgs object containging the received data.</param>
        public delegate void DataReceivedCallback(NetConnection<T> sender, DataEventArgs e);

        /// <summary>
        /// Represents a callback used to inform a listener that a ServerConnection has disconnected.
        /// </summary>
        /// <param name="sender">The sender of the callback.</param>
        /// <param name="e">The SocketAsyncEventArgs object used by the ServerConnection.</param>
        public delegate void DisconnectedCallback(NetConnection<T> sender, SocketAsyncEventArgs e);

        #region Internal Classes

        internal class State
        {
            public DataReceivedCallback dataReceived;
            public DisconnectedCallback disconnectedCallback;
            public Socket socket;
        }

        #endregion Internal Classes

        #region Fields

        private SocketAsyncEventArgs eventArgs;

        public PackageHandler<NetConnection<T>> packageHandler;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// A connection to our server, always listening asynchronously.
        /// </summary>
        /// <param name="socket">The Socket for the connection.</param>
        /// <param name="args">The SocketAsyncEventArgs for asyncronous recieves.</param>
        /// <param name="dataReceived">A callback invoked when data is recieved.</param>
        /// <param name="disconnectedCallback">A callback invoked on disconnection.</param>
        public NetConnection(Socket socket, SocketAsyncEventArgs args, DataReceivedCallback dataReceived,
            DisconnectedCallback disconnectedCallback, T session)
        {
            lock (this)
            {
                this.packageHandler = new PackageHandler<NetConnection<T>>(this);
                State state = new State()
                {
                    socket = socket,
                    dataReceived = dataReceived,
                    disconnectedCallback = disconnectedCallback
                };
                eventArgs = new SocketAsyncEventArgs();
                eventArgs.AcceptSocket = socket;
                eventArgs.Completed += ReceivedCompleted;
                eventArgs.UserToken = state;
                eventArgs.SetBuffer(new byte[64 * 1024], 0, 64 * 1024);

                BeginReceive(eventArgs);
                this.session = session;
            }
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Disconnects the client.
        /// </summary>
        public void Disconnect()
        {
            lock (this)
            {
                CloseConnection(eventArgs);
            }
        }

        /// <summary>
        /// Sends data to the client.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="offset">The offset into the data.</param>
        /// <param name="count">The ammount of data to send.</param>
        public void SendData(Byte[] data, Int32 offset, Int32 count)
        {
            lock (this)
            {
                State state = eventArgs.UserToken as State;
                Socket socket = state.socket;
                if (socket.Connected)
                    //socket.Send(data, offset, count, SocketFlags.None);
                    socket.BeginSend(data, 0, count, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            }
        }

        public void SendResponse()
        {
            byte[] data = session.GetResponse();
            this.SendData(data, 0, data.Length);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Starts and asynchronous recieve.
        /// </summary>
        /// <param name="args">The SocketAsyncEventArgs to use.</param>
        private void BeginReceive(SocketAsyncEventArgs args)
        {
            lock (this)
            {
                Socket socket = (args.UserToken as State).socket;
                if (socket.Connected)
                {
                    args.AcceptSocket.ReceiveAsync(args);
                    /*
                    socket.InvokeAsyncMethod(new SocketAsyncMethod(socket.ReceiveAsync),
                        ReceivedCompleted, args);*/
                }
            }
        }

        /// <summary>
        /// Called when an asynchronous receive has completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The SocketAsyncEventArgs for the operation.</param>
        private void ReceivedCompleted(Object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred == 0)
            {
                CloseConnection(args); //Graceful disconnect
                return;
            }
            if (args.SocketError != SocketError.Success)
            {
                CloseConnection(args); //NOT graceful disconnect
                return;
            }

            State state = args.UserToken as State;

            Byte[] data = new Byte[args.BytesTransferred];
            Array.Copy(args.Buffer, args.Offset, data, 0, data.Length);
            OnDataReceived(data, args.RemoteEndPoint as IPEndPoint, state.dataReceived);

            BeginReceive(args);
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <param name="args">The SocketAsyncEventArgs for the connection.</param>
        private void CloseConnection(SocketAsyncEventArgs args)
        {
            State state = args.UserToken as State;
            Socket socket = state.socket;
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch { } // throws if client process has already closed
            socket.Close();
            socket = null;

            args.Completed -= ReceivedCompleted; //MUST Remember This!
            OnDisconnected(args, state.disconnectedCallback);
        }

        #endregion Private Methods

        #region Events

        /// <summary>
        /// Fires the DataReceivedCallback.
        /// </summary>
        /// <param name="data">The data which was received.</param>
        /// <param name="remoteEndPoint">The address the data came from.</param>
        /// <param name="callback">The callback.</param>
        private void OnDataReceived(Byte[] data, IPEndPoint remoteEndPoint, DataReceivedCallback callback)
        {
            callback(this, new DataEventArgs() { RemoteEndPoint = remoteEndPoint, Data = data, Offset = 0, Length = data.Length });
        }

        /// <summary>
        /// Fires the DisconnectedCallback.
        /// </summary>
        /// <param name="args">The SocketAsyncEventArgs for this connection.</param>
        /// <param name="callback">The callback.</param>
        private void OnDisconnected(SocketAsyncEventArgs args, DisconnectedCallback callback)
        {
            callback(this, args);
        }

        #endregion Events

        #region public Property

        /// <summary>
        /// 获取或设置连接的认证状态
        /// true : 已认证
        /// false : 未认证
        /// </summary>
        public bool Verified { get; set; }

        private T session;

        /// <summary>
        /// 获取或设置一个会话对象
        /// </summary>
        public T Session
        { get { return session; } }

        #endregion public Property
    }
}