using GameServer.Entities;
using ITXCM;
using SkillBridge.Message;
using System.Collections.Generic;

namespace GameServer.Managers
{
    /// <summary>
    /// 角色管理器
    /// </summary>
    internal class CharacterManager : Singleton<CharacterManager>
    {
        public Dictionary<int, Character> Characters = new Dictionary<int, Character>();

        public CharacterManager()
        {
        }

        public void Dispose()
        {
        }

        public void Init()
        {
        }

        public void Clear() => Characters.Clear();

        // 添加角色
        public Character AddCharacter(TCharacter cha)
        {
            Character character = new Character(CharacterType.Player, cha);
            EntityManager.Instance.AddEntity(cha.MapID, character);
            character.Info.EntityId = character.EntityId; // 赋值实体id 必须在添加实体后赋值
            Characters[character.Id] = character;
            return character;
        }

        // 移除角色
        public void RemoveCharacter(int characterId)
        {
            if (Characters.ContainsKey(characterId))
            {
                var cha = Characters[characterId];
                EntityManager.Instance.RemoveEntity(cha.Data.MapID, cha);
                Characters.Remove(characterId);
            }
        }

        // 获取角色
        public Character GetCharacter(int characterId)
        {
            Characters.TryGetValue(characterId, out Character character);
            return character;
        }

        // 数据库角色转换本地
        public Character GetCharacter(TCharacter cha)
        {
            return new Character(CharacterType.Player, cha);
        }
    }
}