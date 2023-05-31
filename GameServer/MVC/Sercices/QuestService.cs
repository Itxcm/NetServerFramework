using GameServer.Entities;
using GameServer.Managers;
using GameServer.Network;
using ITXCM;
using SkillBridge.Message;

namespace GameServer.Services
{
    internal class QuestService : Singleton<QuestService>
    {
        public QuestService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<QuestAcceptRequest>(Recv_QuestAccept);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<QuestSubmitRequest>(Recv_QuestSubmit);
        }

        // 接收任务接受
        private void Recv_QuestAccept(NetConnection<NetSession> client, QuestAcceptRequest ret)
        {
            Character character = client.Session.Character;
            Log.Info($"Recv_QuestAccept: characterID:{character.Id} QuestId{ret.QuestId}");
            QuestAcceptResponse msg = QuestManager.Instance.AcceptQuest(character, ret.QuestId);
            client.Session.Response.questAccept = msg;
            client.SendResponse();
        }

        // 接收任务提交
        private void Recv_QuestSubmit(NetConnection<NetSession> client, QuestSubmitRequest ret)
        {
            Character character = client.Session.Character;
            Log.Info($"Recv_QuestSubmit: characterID:{character.Id} QuestId{ret.QuestId}");
            QuestSubmitResponse msg = QuestManager.Instance.SubmitQuest(character, ret.QuestId);
            client.Session.Response.questSubmit = msg;
            client.SendResponse();
        }

        public void Init()
        {
        }
    }
}