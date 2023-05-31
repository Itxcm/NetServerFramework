using SkillBridge.Message;

namespace ITXCM.Data
{
    public class CharacterDefine
    {
        public int TID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public CharacterClass Class { get; set; }
        public string Resource { get; set; }
        public string Description { get; set; }
        public int InitLevel { get; set; }
        public int Speed { get; set; }
    }
}