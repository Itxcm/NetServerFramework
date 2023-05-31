using ITXCM.Construct;
using ITXCM.Data;
using SkillBridge.Message;

namespace GameServer.Entities
{
    public class CharacterBase : Entity
    {
        public int Id; // 标识id
        public string Name => Info.Name; //  角色名称

        public CharacterDefine Define; // 角色配置表

        public NCharacterInfo Info; // 角色传输信息

        public CharacterBase(Vector3Int pos, Vector3Int dir) : base(pos, dir)
        {
        }

        public CharacterBase(CharacterType type, int configId, int level, Vector3Int pos, Vector3Int dir) : base(pos, dir)
        {
            Define = DataManager.Instance.Characters[configId];
            Info = new NCharacterInfo
            {
                Type = type,
                Level = level,
                ConfigId = configId,
                Entity = EntityData, // TODO 这里协议该清楚点
                EntityId = EntityId,
                Name = Define.Name
            };
        }
    }
}