using GameServer.Models;
using ITXCM;
using ITXCM.Data;

namespace GameServer.Managers
{
    public class Spawner
    {
        public SpawnRuleDefine Define { get; set; } // 刷怪规则
        private SpawnPointDefine spawnPointInfo = null; // 刷怪点

        private Map Map; // 刷怪地图

        private float spawnTime = 0; // 刷怪时间
        private float unSpawnTime = 0; // 消失时间

        private bool spawned = false; //是否刷过

        public Spawner(SpawnRuleDefine def, Map map)
        {
            Define = def;
            Map = map;

            if (DataManager.Instance.SpawnPoints.ContainsKey(Map.ID)) // 判断地图是否包含刷怪规则
            {
                if (DataManager.Instance.SpawnPoints[Map.ID].ContainsKey(Define.SpawnPoint)) // 判断刷怪规则的刷怪点是否存在
                {
                    spawnPointInfo = DataManager.Instance.SpawnPoints[Map.ID][Define.SpawnPoint];
                }
                else Log.ErrorFormat($"SpawnRule{Define.ID} SpawnPoint{Define.SpawnPoint} not existed");
            }
        }

        public void Update()
        {
            if (CanSpawn()) Spawn();
        }

        // 刷怪判断
        private bool CanSpawn()
        {
            if (spawned) return false;
            if (unSpawnTime + 30.0f > TimeUtil.time) return false; // 没到刷怪时间
            return true;
        }

        // 刷怪
        private void Spawn()
        {
            spawned = true;
            Log.InfoFormat($"Map:{Define.MapID} Spawn:[{Define.ID} Monster:{Define.SpawnMonID} lv:{Define.SpawnLevel}] AtPoint:{Define.SpawnPoint}");
            Map.MonsterManger.Create(Define.SpawnMonID, Define.SpawnLevel, spawnPointInfo.Position, spawnPointInfo.Direction);
        }
    }
}