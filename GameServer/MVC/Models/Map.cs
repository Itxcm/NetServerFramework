using GameServer.Entities;
using GameServer.Managers;
using GameServer.Network;
using GameServer.Services;
using ITXCM;
using ITXCM.Data;
using SkillBridge.Message;
using System.Collections.Generic;

namespace GameServer.Models
{
    public class Map
    {
        // 地图角色定义
        internal class MapCharacter
        {
            public NetConnection<NetSession> client;
            public Character character;

            public MapCharacter(NetConnection<NetSession> conn, Character cha)
            {
                client = conn;
                character = cha;
            }
        }

        private Dictionary<int, MapCharacter> MapCharacters = new Dictionary<int, MapCharacter>(); // 地图角色 以entityId为key

        public MapDefine Define; // 地图定义
        public SpawnManger SpawnManger = new SpawnManger(); // 刷怪管理器
        public MonsterManger MonsterManger = new MonsterManger(); // 怪物管理器

        public int ID => Define.ID; // 地图id

        internal Map(MapDefine define)
        {
            Define = define;
            SpawnManger.Init(this);
            MonsterManger.Init(this);
        }

        internal void Update()
        {
            SpawnManger.Update();
        }

        //角色进入地图
        internal void CharacterEnter(NetConnection<NetSession> client, Character character)
        {
            // 打印日志 标记角色的地图id 添加地图中的角色
            Log.InfoFormat("CharacterEnter: Map:{0} characterId:{1}", Define.ID, character.Id);

            // 处理实体数据和 模型数据
            character.Info.mapId = ID;
            MapCharacters[character.EntityId] = new MapCharacter(client, character);

            // 创建响应协议 赋值
            client.Session.Response.mapCharacterEnter = new MapCharacterEnterResponse { mapId = Define.ID };

            // 遍历地图角色 添加响应的地图角色  通知其他人
            foreach (var mapCharacter in MapCharacters.Values)
            {
                client.Session.Response.mapCharacterEnter.Characters.Add(mapCharacter.character.Info);
                if (mapCharacter.character != character) SendCharacterEnterMap(mapCharacter.client, character.Info);
            }
            // 处理自身
            client.SendResponse();
        }

        // 角色离开地图
        internal void CharacterLeave(Character character)
        {
            // 打印日志 标记角色的地图id 移除地图中的角色
            Log.InfoFormat("CharacterLeave: Map:{0} characterId:{1}", Define.ID, character.Id);
            // 遍历地图角色 添加响应的地图角色  通知所有人
            foreach (var mapCharacter in MapCharacters.Values) SendCharacterLeaveMap(mapCharacter.client, character);
            if (MapCharacters.ContainsKey(character.EntityId)) MapCharacters.Remove(character.EntityId);
        }

        // 角色地图中更新
        internal void UpdateEntity(NEntitySync entity)
        {
            foreach (var mapCharacter in MapCharacters.Values)
            {
                var character = mapCharacter.character;
                // 是自己更新位置给服务器 不是自己则发给这个存在的人
                if (character.EntityId == entity.Id)
                {
                    character.Position = entity.Entity.Position;
                    character.Direction = entity.Entity.Direction;
                    character.Speed = entity.Entity.Speed;
                }
                else MapService.Instance.SendEntityUpdate(mapCharacter.client, entity);
            }
        }

        // 怪物进入地图
        internal void MonsterEnter(Monster monster)
        {
            Log.InfoFormat($"MonsterEnter: mapId:{Define.ID} monsterId:{monster.EntityId}");

            foreach (var kv in MapCharacters) // 怪物属于角色分支
            {
                SendCharacterEnterMap(kv.Value.client, monster.Info);
            }
        }

        // 添加角色进入的 其他人响应
        internal void SendCharacterEnterMap(NetConnection<NetSession> client, NCharacterInfo character)
        {
            Log.InfoFormat("SendCharacterEnterMap: Map:{0} characterId:{1}", Define.ID, character.Id);
            if (client.Session.Response.mapCharacterEnter == null) client.Session.Response.mapCharacterEnter = new MapCharacterEnterResponse { mapId = Define.ID };
            client.Session.Response.mapCharacterEnter.Characters.Add(character);
            client.SendResponse();
        }

        // 添加角色离开的 所有人响应
        internal void SendCharacterLeaveMap(NetConnection<NetSession> client, Character character)
        {
            Log.InfoFormat("SendCharacterLeaveMap To {0}:{1} : Map:{2} Character:{3}:{4}", client.Session.Character.Id, client.Session.Character.Info.Name, Define.ID, character.Id, character.Info.Name);
            client.Session.Response.mapCharacterLeave = new MapCharacterLeaveResponse { entityId = character.EntityId };
            client.SendResponse();
        }
    }
}