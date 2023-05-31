using GameServer.Managers;
using GameServer.Models;
using GameServer.Network;
using ITXCM;
using ITXCM.Data;
using SkillBridge.Message;

namespace GameServer.Entities
{
    /// <summary>
    /// 玩家角色类
    /// </summary>
    public class Character : CharacterBase, IPostResponser
    {
        public TCharacter Data; // 数据库存储的角色信息 可用于直接修改数据 然后保存去同步修改数据库

        public ItemManager ItemManager; // 道具管理器

        public StatusManager StatusManager; // 状态管理器

        public QuestManager QuestManager; // 任务管理器

        public FriendManager FriendManager; // 好友管理器

        // 当前队伍 只存于缓存 更新时间戳

        public Team Team { get; set; }
        public double TeamUpdateTS;

        // 当前工会 存于数据库 更新时间戳

        public Guild Guild { get; set; }
        public double GuildUpdateTS;

        public Character(CharacterType type, TCharacter cha) :
            base(new ITXCM.Construct.Vector3Int(cha.MapPosX, cha.MapPosY, cha.MapPosZ), new ITXCM.Construct.Vector3Int(100, 0, 0))
        {
            Data = cha;
            Id = cha.ID;

            // 构建网络传输信息
            Info = new NCharacterInfo
            {
                Type = type,
                Id = cha.ID,
                Name = cha.Name,
                Entity = EntityData,
                EntityId = EntityData.Id,
                ConfigId = cha.TID,
                Level = cha.Level,
                Class = (CharacterClass)cha.Class,
                mapId = cha.MapID,
                Ride = 0,
                Gold = cha.Gold,
                Equips = cha.Equips,
                Bag = new NBagInfo { Items = Data.Bag.Items, Unlocked = Data.Bag.Unlocked },
            };

            Define = DataManager.Instance.Characters[Info.ConfigId];

            // 道具管理
            ItemManager = new ItemManager(this);
            ItemManager.GetItemInfos(Info.Items);

            // 状态管理
            StatusManager = new StatusManager(this);

            // 任务管理
            QuestManager = new QuestManager(this);
            QuestManager.GetQuestInfos(Info.Quests);

            // 好友管理
            FriendManager = new FriendManager(this);
            FriendManager.GetFriendInfos(Info.Friends);

            // 工会
            Guild = GuildManager.Instance.GetGuildById(Data.GuildId);
        }

        // 金币变化
        public long Gold
        {
            get => Data.Gold;
            set
            {
                if (Data.Gold == value) return;
                // 改变了 通知状态管理器 金币状态改变 等session会话一起发过去
                StatusManager.AddGoldChange((int)(value - Data.Gold)); // 新金币 - 老金币 等于变化值
                Data.Gold = value;
            }
        }

        // 状态批处理
        public void PostProcess(NetMessageResponse message)
        {
            Log.InfoFormat("PostProcess > Character: characterID:{0}:{1}", Id, Info.Name);

            FriendManager.PostProcess(message);

            // 队伍处理
            if (Team != null)
            {
                if (TeamUpdateTS < Team.timeStamp) // 发现队伍更新时间戳 小于当前时间戳 更新队伍信息(保证每个人都能收到消息)
                {
                    Log.InfoFormat($"PostProcess > Team: characterID:{Id}:{Info.Name} {TeamUpdateTS}<{Team.timeStamp}");
                    TeamUpdateTS = Team.timeStamp;
                    Team.PostProcess(message);
                }
            }
            // 工会处理
            if (Guild != null)
            {
                Info.Guild = Guild.GetGuildInfo(this); // 刷新工会信息

                if (GuildUpdateTS < Guild.timeStamp)
                {
                    Log.InfoFormat($"PostProcess > Guild: characterID:{Id}:{Info.Name} {GuildUpdateTS}<{Guild.timeStamp}");
                    GuildUpdateTS = Guild.timeStamp;
                    Guild.PostProcess(this, message);
                }
            }

            // 状态处理
            if (StatusManager.HasStatus) StatusManager.PostProcess(message);
        }

        // 角色离开时调用
        public void Clear() => FriendManager.OfflineNotify();

        // 获取角色传输信息 部分用 只需要部分
        public NCharacterInfo GetBasicInfo()
        {
            return new NCharacterInfo
            {
                Id = Id,
                Name = Info.Name,
                Class = Info.Class,
                Level = Info.Level,
            };
        }
    }
}