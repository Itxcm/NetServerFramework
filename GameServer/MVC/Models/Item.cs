namespace GameServer.Models
{
    public class Item
    {
        private TCharacterItem DbItem;
        public int ItemID;
        public int Count;

        public Item(TCharacterItem item)
        {
            DbItem = item;
            ItemID = item.ItemID;
            Count = item.ItemCount;
        }

        // 添加物品
        public void Add(int count)
        {
            Count += count;
            DbItem.ItemCount = Count;
        }

        // 删除物品
        public void Remove(int count)
        {
            Count -= count;
            DbItem.ItemCount = Count;
        }

        // 使用物品
        public bool Use(int count = 1) => false;

        // 重载打印
        public override string ToString() => string.Format("ID:{0},Count:{1}", ItemID, Count);
    }
}