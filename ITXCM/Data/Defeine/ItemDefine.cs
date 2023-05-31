using SkillBridge.Message;
using System.Collections.Generic;

namespace ITXCM.Data
{
    public enum ItemFunction
    {
        RecoverHP,
        RecoverMP,
        AddBuff,
        AddExp,
        AddMoney,
        AddItem,
        AddSkillPoint,
    }

    public class ItemDefine
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemType Type { get; set; } // 道具类型
        public string Category { get; set; }
        public int Level { get; set; }
        public CharacterClass LimitClass { get; set; }
        public int StackLimit { get; set; } // 堆叠限制
        public bool CanUse { get; set; }
        public float UseCD { get; set; }
        public int Price { get; set; }
        public int SellPrice { get; set; }
        public string Icon { get; set; }
        public ItemFunction Function { get; set; } // 道具功能
        public int Param { get; set; }
        public List<int> Params { get; set; }
    }
}