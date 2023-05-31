using GameServer.Entities;
using GameServer.Models;
using SkillBridge.Message;
using System.Collections.Generic;

namespace GameServer.Managers
{
    public class MonsterManger
    {
        private Map map; // 当前地图
        public Dictionary<int, Monster> Monsters = new Dictionary<int, Monster>(); // 所有怪物

        public void Init(Map map)
        {
            this.map = map;
        }

        // 创建怪物
        public Monster Create(int spawnMonID, int spawnLevel, NVector3 pos, NVector3 dir)
        {
            Monster monster = new Monster(map.ID, spawnMonID, spawnLevel, pos, dir);
            EntityManager.Instance.AddEntity(map.ID, monster);
            monster.Info.EntityId = monster.EntityId;
            Monsters[monster.EntityId] = monster;

            map.MonsterEnter(monster);

            return monster;
        }

        // 发送所有已存在怪物进入
        public void SendAllAliveMonster()
        {
            foreach (var monster in Monsters.Values) map.MonsterEnter(monster);
        }
    }
}