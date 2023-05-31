using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace ITXCM.Data
{
    public class DataManager : Singleton<DataManager>
    {
        public string DataPath; //TODO PC安卓路径还没想好解决方案 
        public Dictionary<int, MapDefine> Maps = null;
        public Dictionary<int, CharacterDefine> Characters = null;
        public Dictionary<int, TeleporterDefine> Teleporters = null;
        public Dictionary<int, Dictionary<int, SpawnPointDefine>> SpawnPoints = null;
        public Dictionary<int, Dictionary<int, SpawnRuleDefine>> SpawnRules = null;
        public Dictionary<int, NpcDefine> Npcs = null;
        public Dictionary<int, ItemDefine> Items = null;
        public Dictionary<int, EquipDefine> Equips = null;
        public Dictionary<int, ShopDefine> Shops = null;
        public Dictionary<int, QuestDefine> Quests = null;
        public Dictionary<int, Dictionary<int, ShopItemDefine>> ShopItems = null;

        public DataManager()
        {
            DataPath = "Data/"; // 先默认Data 服务器需要在Debug目录加
            Log.Info("DataManager > DataManager()");
        }

        public void Load()
        {
            string json = File.ReadAllText(DataPath + "MapDefine.txt");
            Maps = JsonConvert.DeserializeObject<Dictionary<int, MapDefine>>(json);

            json = File.ReadAllText(DataPath + "CharacterDefine.txt");
            Characters = JsonConvert.DeserializeObject<Dictionary<int, CharacterDefine>>(json);

            json = File.ReadAllText(DataPath + "TeleporterDefine.txt");
            Teleporters = JsonConvert.DeserializeObject<Dictionary<int, TeleporterDefine>>(json);

            json = File.ReadAllText(DataPath + "SpawnPointDefine.txt");
            SpawnPoints = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, SpawnPointDefine>>>(json);

            json = File.ReadAllText(DataPath + "SpawnRuleDefine.txt");
            SpawnRules = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, SpawnRuleDefine>>>(json);

            json = File.ReadAllText(DataPath + "NpcDefine.txt");
            Npcs = JsonConvert.DeserializeObject<Dictionary<int, NpcDefine>>(json);

            json = File.ReadAllText(DataPath + "ItemDefine.txt");
            Items = JsonConvert.DeserializeObject<Dictionary<int, ItemDefine>>(json);

            json = File.ReadAllText(DataPath + "EquipDefine.txt");
            Equips = JsonConvert.DeserializeObject<Dictionary<int, EquipDefine>>(json);

            json = File.ReadAllText(DataPath + "ShopDefine.txt");
            Shops = JsonConvert.DeserializeObject<Dictionary<int, ShopDefine>>(json);

            json = File.ReadAllText(DataPath + "QuestDefine.txt");
            Quests = JsonConvert.DeserializeObject<Dictionary<int, QuestDefine>>(json);

            json = File.ReadAllText(DataPath + "ShopItemDefine.txt");
            ShopItems = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, ShopItemDefine>>>(json);
        }
    }
}