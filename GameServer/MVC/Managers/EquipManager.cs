using GameServer.Entities;
using ITXCM;
using SkillBridge.Message;
using GameServer.Services;

namespace GameServer.Managers
{
    internal class EquipManager : Singleton<EquipManager>
    {
        // 装备道具
        public ItemEquipResponse EquipItem(Character cha, int slot, int itemId, bool isEquip)
        {
            ItemEquipResponse msg = new ItemEquipResponse();

            if (!cha.ItemManager.Items.ContainsKey(itemId))
            {
                msg.Result = Result.Failed;
                msg.Errormsg = "您没有这个道具!";
                return msg;
            }

            UpdateEquip(cha.Data.Equips, slot, itemId, isEquip);
            DBService.Instance.Save();

            return msg;
        }

        // 更新装备数据
        private unsafe void UpdateEquip(byte[] equipData, int slot, int itemId, bool isEquip)
        {
            // 指针指向角色装备数组
            fixed (byte* pt = equipData)
            {
                int* slotid = (int*)(pt + slot * sizeof(int)); // 槽子的指针 当前指针+插槽id*每个插槽占的大小
                if (isEquip) *slotid = itemId; // 穿装备 该指针赋值为道具id
                else *slotid = 0; // 脱装备 道具id归0
            }
        }
    }
}