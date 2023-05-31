using GameServer.Entities;
using GameServer.Managers;
using GameServer.Network;
using ITXCM;
using ITXCM.Data;
using SkillBridge.Message;

namespace GameServer.Services
{
    internal class MapService : Singleton<MapService>
    {
        public MapService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<MapEntitySyncRequest>(Recv_MapEntitySync);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<MapTeleportRequest>(Recv_MapTeleport);
        }

        public void Init() => MapManager.Instance.Init();

        public void Update() => MapManager.Instance.Update();

        // 发送地图实体更新
        public void SendEntityUpdate(NetConnection<NetSession> client, NEntitySync entity)
        {
            //  Log.InfoFormat("SendEntityUpdate: CharacterId:{0}", client.Session.Character.Id);
            client.Session.Response.mapEntitySync = new MapEntitySyncResponse();
            client.Session.Response.mapEntitySync.entitySyncs.Add(entity);
            client.SendResponse();
        }

        // 接收地图实体同步
        private void Recv_MapEntitySync(NetConnection<NetSession> sender, MapEntitySyncRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("Recv_MapEntitySync: characterID:{0}:{1} Entity.Id:{2} Evt:{3} Entity:{4}", character.Id, character.Info.Name, request.entitySync.Id, request.entitySync.Event, request.entitySync.Entity.ToString());
            MapManager.Instance[character.Info.mapId].UpdateEntity(request.entitySync);
        }

        // 接收地图传送
        private void Recv_MapTeleport(NetConnection<NetSession> sender, MapTeleportRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("Recv_MapTeleport: characterID:{0}:{1} TeleporterId:{2}", character.Id, character.Data, request.teleporterId);

            if (!DataManager.Instance.Teleporters.ContainsKey(request.teleporterId))
            {
                Log.WarningFormat("Source TeleporterID [{0}] not existed", request.teleporterId);
                return;
            }
            TeleporterDefine source = DataManager.Instance.Teleporters[request.teleporterId];
            if (source.LinkTo == 0 || !DataManager.Instance.Teleporters.ContainsKey(source.LinkTo))
            {
                Log.WarningFormat("Source TeleporterID [{0}] LinkTo ID [{1}] not existed", request.teleporterId, source.LinkTo);
            }

            TeleporterDefine target = DataManager.Instance.Teleporters[source.LinkTo];

            MapManager.Instance[source.MapID].CharacterLeave(character);
            character.Position = target.Position;
            character.Direction = target.Direction;
            MapManager.Instance[target.MapID].CharacterEnter(sender, character);

            MapManager.Instance[target.MapID].MonsterManger.SendAllAliveMonster();
        }
    }
}