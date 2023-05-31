using GameServer.Entities;
using GameServer.Network;
using ITXCM;
using GameServer.Services;
using SkillBridge.Message;

namespace GameServer.Services
{
    internal class BagService
    {
        public BagService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<BagSaveRequest>(Recv_BagSave);
        }

        // 接收背包保存
        private void Recv_BagSave(NetConnection<NetSession> client, BagSaveRequest request)
        {
            Character character = client.Session.Character;
            Log.InfoFormat("Recv_BagSave: characterID:{0} Unlocked:{1}", character.Id, request.BagInfo.Unlocked);

            if (request.BagInfo != null)
            {
                character.Data.Bag.Items = request.BagInfo.Items;
                DBService.Instance.Save();
            }
        }

        public void Init()
        { }
    }
}