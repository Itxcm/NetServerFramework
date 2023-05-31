using ITXCM;
using GameServer.Entities;
using System.Collections.Generic;

namespace GameServer.Managers
{
    public class EntityManager : Singleton<EntityManager>
    {
        private int idx = 0; // 实体id
        public List<Entity> AllEntities = new List<Entity>(); //  所有实体 整个服务器存在的
        public Dictionary<int, List<Entity>> MapEntities = new Dictionary<int, List<Entity>>(); // 地图中存在实体

        // 添加实体
        public void AddEntity(int mapId, Entity entity)
        {
            AllEntities.Add(entity);

            // 加入管理器生成唯一id
            entity.EntityData.Id = ++idx;

            // 判断地图的列表是否为空 为空则创建一个
            if (!MapEntities.TryGetValue(mapId, out List<Entity> entities))
            {
                entities = new List<Entity>();
                MapEntities[mapId] = entities;
            }
            entities.Add(entity);
        }

        // 删除实体
        public void RemoveEntity(int mapId, Entity entity)
        {
            // 删除整个服务器的 删除地图的
            if (AllEntities.Contains(entity)) AllEntities.Remove(entity);
            if (MapEntities[mapId].Contains(entity)) MapEntities[mapId].Remove(entity);
        }
    }
}