using GameServer.Network;
using ITXCM;
using System.Collections.Generic;

namespace GameServer.Managers
{
    public class SessionManager : Singleton<SessionManager>
    {
        private Dictionary<int, NetConnection<NetSession>> clientDic = new Dictionary<int, NetConnection<NetSession>>(); // 连接对象字典 角色id-客户端

        public void AddSession(int characterId, NetConnection<NetSession> client) => clientDic[characterId] = client; // 添加连接

        public void RemoveSession(int characterId) // 移除连接
        {
            if (clientDic.ContainsKey(characterId)) clientDic.Remove(characterId);
        }

        public NetConnection<NetSession> GetSession(int characterId) // 获取连接
        {
            clientDic.TryGetValue(characterId, out NetConnection<NetSession> client);
            return client;
        }
    }
}