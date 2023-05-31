using GameServer.Entities;
using ITXCM;
using ITXCM.Data;
using GameServer.Services;
using SkillBridge.Message;

namespace GameServer.Managers
{
    internal class ShopManager : Singleton<ShopManager>
    {
        // 购买商品
        public ItemBuyResponse BuyItem(Character cha, int shopId, int shopItemId)
        {
            ItemBuyResponse msg = new ItemBuyResponse();

            if (!DataManager.Instance.Shops.ContainsKey(shopId))
            {
                msg.Result = Result.Failed;
                msg.Errormsg = "非法商店id";
                return msg;
            }

            if (DataManager.Instance.ShopItems[shopId].TryGetValue(shopItemId, out ShopItemDefine shopItem))
            {
                Log.InfoFormat("BuyItem: characterID:{0} Item:{1} Count:{2} Price:{3} ", cha.Id, shopItem.ItemID, shopItem.Count, shopItem.Price);

                // 金币效验
                if (cha.Gold >= shopItem.Price)
                {
                    cha.ItemManager.AddItem(shopItem.ItemID, shopItem.Count); // 道具变化
                    cha.Gold -= shopItem.Price; // 金币变化

                    msg.Result = Result.Success;
                    msg.Errormsg = "购买成功!";

                    DBService.Instance.Save(); // 保存变化到数据库
                }
                else
                {
                    msg.Result = Result.Failed;
                    msg.Errormsg = "没有足够的金币哦!";
                }
                return msg;
            }
            else
            {
                msg.Result = Result.Failed;
                msg.Errormsg = "非法商品id";
                return msg;
            }
        }
    }
}