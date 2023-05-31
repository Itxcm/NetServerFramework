using GameServer.Models;
using ITXCM;
using ITXCM.Data;
using System.Collections.Generic;

namespace GameServer.Managers
{
    /// <summary>
    /// 地图管理器
    /// </summary>
    internal class MapManager : Singleton<MapManager>
    {
        private Dictionary<int, Map> Maps = new Dictionary<int, Map>();

        public void Init()
        {
            foreach (var mapdefine in DataManager.Instance.Maps.Values)
            {
                Map map = new Map(mapdefine);
                Log.InfoFormat("MapManager.Init > Map:{0}:{1}", map.Define.ID, map.Define.Name);
                Maps[mapdefine.ID] = map;
            }
        }

        public Map this[int key] => Maps[key];

        public void Update()
        {
            foreach (var map in this.Maps.Values) map.Update();
        }
    }
}