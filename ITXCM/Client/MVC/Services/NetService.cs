using SkillBridge.Message;
using System;
using UnityEngine;

namespace ITXCM.Client.Services
{
    /// <summary>
    ///  网络服务层
    /// </summary>
    public class NetService : Singleton<NetService>, IDisposable
    {
        public Action ServerDisConnectNoMsg; // 服务器断开 无消息在发 MessageBox.Show("网络错误，无法连接到服务器!", "错误", MessageBoxType.Error);
        public Action ServerDisconnectHasMsg; // 服务器断开 有消息在发 MessageBox.Show("服务器断开!", "错误", MessageBoxType.Error);
        public Action ServerConnectSucess; // 服务器连接/重连成功

        // 上一次发送的网络消息
        private NetMessage pendingMessage = null;

        // 连接状态
        private bool connected = false;

        public void Init()
        { }

        public NetService()
        {
            NetClient.Instance.OnConnect += OnGameServerConnect;
            NetClient.Instance.OnDisconnect += OnGameServerDisconnect;
        }

        public void Dispose()
        {
            NetClient.Instance.OnConnect -= OnGameServerConnect;
            NetClient.Instance.OnDisconnect -= OnGameServerDisconnect;
        }

        // 服务器连接
        private void OnGameServerConnect(int result, string reason)
        {
            Log.InfoFormat("LoadingMesager::OnGameServerConnect :{0} reason:{1}", result, reason);
            if (NetClient.Instance.Connected)
            {
                ServerConnectSucess?.Invoke();
                connected = true;
                if (pendingMessage != null)
                {
                    NetClient.Instance.SendMessage(this.pendingMessage);
                    pendingMessage = null;
                }
            }
            else if (!DisconnectNotify(result, reason))
            {
                Debug.LogFormat("RESULT:{0} ERROR:{1} Message: No PendingMessage", result, reason);
                ServerDisConnectNoMsg?.Invoke();
            }
        }

        // 服务器断开连接
        private void OnGameServerDisconnect(int result, string reason) => DisconnectNotify(result, reason);

        // 断开连接通知
        private bool DisconnectNotify(int result, string reason)
        {
            if (pendingMessage != null)
            {
                Debug.LogFormat("RESULT:{0} ERROR:{1} Message:{2}", result, reason, pendingMessage);
                ServerDisconnectHasMsg?.Invoke();
                return true;
            }
            return false;
        }

        // 检测网络并发送
        public void CheckConnentAndSend(NetMessage msg)
        {
            if (connected && NetClient.Instance.Connected)
            {
                pendingMessage = null;
                NetClient.Instance.SendMessage(msg);
            }
            else
            {
                pendingMessage = msg;
                ReConnectToServer();
            }
        }

        #region 外部方法

        // 重接服务器
        public void ReConnectToServer()
        {
            Debug.Log("ReConnectToServer...");
            NetClient.Instance.Init(NetClient.Instance.IP, NetClient.Instance.Port);
            NetClient.Instance.Connect();
        }

        #endregion 外部方法
    }
}