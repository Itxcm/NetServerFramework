using GameServer.Models;
using ITXCM.Data;
using System.Collections.Generic;

namespace GameServer.Managers
{
    public class SpawnManger
    {
        private List<Spawner> Rules = new List<Spawner>(); // 刷怪器列表
        private Map Map;

        public void Init(Map map)
        {
            Map = map;
            if (DataManager.Instance.SpawnRules.ContainsKey(Map.ID)) // 判断地图是否有刷怪规则 有就加刷怪器
            {
                foreach (var def in DataManager.Instance.SpawnRules[Map.ID].Values) Rules.Add(new Spawner(def, Map));
            }
        }

        // 刷怪更新
        public void Update()
        {
            if (Rules.Count == 0) return;
            for (int i = 0; i < Rules.Count; i++) Rules[i].Update(); // 有规则按规则更新
        }
    }
}