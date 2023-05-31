using ITXCM.Construct;
using ITXCM.Data;
using SkillBridge.Message;

namespace GameServer.Entities
{
    public class Monster : CharacterBase
    {
        public Monster(int mapId, int configId, int level, Vector3Int pos, Vector3Int dir) : base(CharacterType.Monster, configId, level, pos, dir)
        {
            Define = DataManager.Instance.Characters[configId];
            Info = new NCharacterInfo
            {
                Type = CharacterType.Monster,
                Id = EntityId,
                Name = Define.Name,
                Entity = EntityData,
                EntityId = EntityId,
                ConfigId = configId,
                Level = level,
                Class = Define.Class,
                mapId = mapId,
            };
        }
    }
}