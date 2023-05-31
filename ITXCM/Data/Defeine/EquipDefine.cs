using SkillBridge.Message;

namespace ITXCM.Data
{
    public class EquipDefine
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public EquipSlot Slot { get; set; }
        public string Category { get; set; }
        public float MaxHP { get; set; }
        public float MaxMP { get; set; }
        public float STR { get; set; }
        public float INT { get; set; }
        public float DEX { get; set; }
        public float AD { get; set; }
        public float AP { get; set; }
        public float DEF { get; set; }
        public float MDEF { get; set; }
        public float SPD { get; set; }
        public float CRI { get; set; }
    }
}