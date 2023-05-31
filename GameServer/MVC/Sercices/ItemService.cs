using GameServer.Entities;
using GameServer.Managers;
using GameServer.Network;
using ITXCM;
using SkillBridge.Message;

namespace GameServer.Services
{
    internal class ItemService : Singleton<ItemService>
    {
        public ItemService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ItemBuyRequest>(Recv_ItemBuy);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ItemEquipRequest>(Recv_ItemEquip);
        }

        // 接收商品购买
        private void Recv_ItemBuy(NetConnection<NetSession> client, ItemBuyRequest request)
        {
            Character character = client.Session.Character;
            Log.InfoFormat("Recv_ItemBuy: characterID:{0}:{1} Shop:{2} ShopItem:{3} ", character.Id, character.Info.Name, request.shopId, request.shopItemId);
            ItemBuyResponse res = ShopManager.Instance.BuyItem(character, request.shopId, request.shopItemId);
            client.Session.Response.itemBuy = res;
            client.SendResponse();
        }

        // 接收道具装备
        private void Recv_ItemEquip(NetConnection<NetSession> client, ItemEquipRequest request)
        {
            Character character = client.Session.Character;
            Log.InfoFormat("Recv_ItemEquip: characterID:{0}:{1} Item:{2} EquipID:{3} ", character.Id, character.Info.Name, request.Slot, request.itemId);
            ItemEquipResponse res = EquipManager.Instance.EquipItem(character, request.Slot, request.itemId, request.isEquip);
            client.Session.Response.itemEquip = res;
            client.SendResponse();
        }

        public void Init()
        { }
    }
}