using GameServer.Entities;
using GameServer.Models;
using ITXCM;
using SkillBridge.Message;
using System.Collections.Generic;

namespace GameServer.Managers
{
    public class ItemManager
    {
        private Character Owner;

        public Dictionary<int, Item> Items = new Dictionary<int, Item>(); // 物品id 对应 物品

        public ItemManager(Character owner)
        {
            Owner = owner;
            // 构造时将角色数据库中的道具添加到Items
            foreach (var item in owner.Data.Items) Items.Add(item.ItemID, new Item(item));
        }

        // 使用物品
        public bool UseItem(int itemId, int count = 1)
        {
            Log.InfoFormat("UseItem: CharacterId[{0}] ItemId[{1}] Count[{2}]", Owner.Data.ID, itemId, count);
            if (Items.TryGetValue(itemId, out Item item))
            {
                if (item.Count < count) return false; // 使用数量大于存在数量 数量不足

                // TODO 使用逻辑

                item.Remove(count);

                return true;
            }
            return false;
        }

        // 获取道具
        public Item GetItem(int itemId)
        {
            Items.TryGetValue(itemId, out Item item);
            Log.InfoFormat("GetItem: CharacterId[{0}] ItemId[{1}] Item[{2}]", Owner.Data.ID, itemId, item);
            return item;
        }

        // 增加道具
        public bool AddItem(int itemId, int count)
        {
            if (Items.TryGetValue(itemId, out Item item)) item.Add(count);
            else
            {
                // 不存在 先根据数据库构造一个
                TCharacterItem dbItem = new TCharacterItem();
                dbItem.CharacterID = Owner.Data.ID;
                dbItem.Owner = Owner.Data;
                dbItem.ItemID = itemId;
                dbItem.ItemCount = count;

                // 添加道具 存到数据库中
                Owner.Data.Items.Add(dbItem);

                // 添加道具 存在缓存中
                item = new Item(dbItem);
                Items.Add(itemId, item);
            }
            Owner.StatusManager.AddItemChange(itemId, count, StatusAction.Add); // 通知状态变化
            Log.InfoFormat("AddItem: CharacterId[{0}] ItemId[{1}] Item[{2}] Count[{3}]", Owner.Data.ID, item.ItemID, item, count);
            return true;
        }

        // 移除道具
        public bool RemoveItem(int itemId, int count)
        {
            if (Items.TryGetValue(itemId, out Item item))
            {
                if (item.Count < count) return false; // 移除数量大于存在数量 无法移除
                item.Remove(count); // 移除道具 数据库存在改道具 不用在数据库移除
                Owner.StatusManager.AddItemChange(itemId, count, StatusAction.Delete); // 通知状态变化
                Log.InfoFormat("RemoveItem: CharacterId[{0}] ItemId[{1}] Item[{2}] Count[{3}]", Owner.Data.ID, item.ItemID, item, count);
                return true;
            }
            return false;
        }

        // 判断是否有该道具 存在且数量大于0
        public bool HasItem(int itemId)
        {
            Items.TryGetValue(itemId, out Item item);
            return item != null && item.Count > 0;
        }

        // 将数据库存储的道具信息赋值给角色
        public void GetItemInfos(List<NItemInfo> list)
        {
            foreach (var item in Items.Values) list.Add(new NItemInfo() { Id = item.ItemID, Count = item.Count });
        }
    }
}