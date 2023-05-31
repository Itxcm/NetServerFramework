using GameServer.Entities;
using GameServer.Services;
using ITXCM;
using SkillBridge.Message;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Managers
{
    public class FriendManager
    {
        private Character Owner;
        private List<NFriendInfo> Friends = new List<NFriendInfo>(); // 好友信息

        private bool friendChanged = false;

        public FriendManager(Character owner)
        {
            Owner = owner;
            InitFriends();
        }

        // 初始化好友
        private void InitFriends()
        {
            Friends.Clear();

            foreach (var friend in Owner.Data.Friends) Friends.Add(GetFriendInfo(friend)); // 缓存数据中添加
        }

        // 后处理
        public void PostProcess(NetMessageResponse message)
        {
            if (friendChanged)
            {
                Log.InfoFormat($"PostProcess > FriendManager : characterID:{Owner.Id} Name{Owner.Info.Name}");
                InitFriends();
                if (message.friendList == null)
                {
                    message.friendList = new FriendListResponse();
                    message.friendList.Friends.AddRange(Friends);
                }
                friendChanged = false;
            }
        }

        #region 外部调用

        // 将当前好友信息存入指定列表
        public void GetFriendInfos(List<NFriendInfo> list)
        {
            foreach (var f in Friends) list.Add(f);
        }

        // 更新好友信息 (暂时只有上线状态)
        public void UpdataFriendInfo(NCharacterInfo friendInfo, int status)
        {
            NFriendInfo info = GetFriendByFriendId(friendInfo.Id);
            if (info != null) info.Status = status;
            friendChanged = true;
        }

        // 添加好友
        public void AddFriend(Character friend)
        {
            TCharacterFriend tf = new TCharacterFriend
            {
                FriendID = friend.Id,
                FriendName = friend.Data.Name,
                Class = friend.Data.Class,
                Level = friend.Data.Level
            };
            Owner.Data.Friends.Add(tf); // 添加到数据库
            friendChanged = true;
        }

        // 删除好友 自己删除好友
        public bool RemoveFriendById(int id)
        {
            var removeItem = Owner.Data.Friends.FirstOrDefault(v => v.Id == id);
            if (removeItem != null) DBService.Instance.Entities.CharacterFriends.Remove(removeItem);
            friendChanged = true;
            return true;
        }

        // 删除好友 好友删除自己
        public bool RemoveFriendByFriendId(int friendId)
        {
            var removeItem = Owner.Data.Friends.FirstOrDefault(v => v.FriendID == friendId);
            if (removeItem != null) DBService.Instance.Entities.CharacterFriends.Remove(removeItem);
            friendChanged = true;
            return true;
        }

        // 获取好友信息
        public NFriendInfo GetFriendByFriendId(int friendId) => Friends.Find(x => x.friendInfo.Id == friendId);

        // 下线通知
        public void OfflineNotify()
        {
            foreach (var info in Friends) // 遍历所有好友
            {
                var friend = CharacterManager.Instance.GetCharacter(info.friendInfo.Id); // 查找好友的角色
                if (friend != null) friend.FriendManager.UpdataFriendInfo(Owner.Info, 0); // 好友在线 则通知他更新我的信息
            }
        }

        #endregion 外部调用

        #region 私有方法简化

        // 根据数据库信息 获取角色传输的好友信息
        private NFriendInfo GetFriendInfo(TCharacterFriend friend)
        {
            NFriendInfo info = new NFriendInfo();
            var character = CharacterManager.Instance.GetCharacter(friend.FriendID); // 获取到这个好友的角色

            info.friendInfo = new NCharacterInfo();
            info.Id = friend.Id; // 标识id(自增字段)

            if (character == null) // 角色不在线 数据库中赋值
            {
                info.friendInfo.Id = friend.FriendID;
                info.friendInfo.Name = friend.FriendName;
                info.friendInfo.Class = (CharacterClass)friend.Class;
                info.friendInfo.Level = friend.Level;
                info.Status = 0;
            }
            else // 缓存数据管理器中赋值
            {
                info.friendInfo = GetBasicInfo(character.Info);
                info.friendInfo.Name = character.Info.Name;
                info.friendInfo.Class = character.Info.Class;
                info.friendInfo.Level = character.Info.Level;

                if (friend.Level != character.Info.Level) friend.Level = character.Info.Level; // 防止角色下线等级变化

                character.FriendManager.UpdataFriendInfo(Owner.Info, 1);
                info.Status = 1;
            }
            Log.InfoFormat($"GetFriendInfo: OwnerId{Owner.Id} Name:{Owner.Info.Name} Status{info.Status}");
            return info;
        }

        // 防止交叉引用 构建一个新对象
        public NCharacterInfo GetBasicInfo(NCharacterInfo info)
        {
            return new NCharacterInfo()
            {
                Id = info.Id,
                Name = info.Name,
                Class = info.Class,
                Level = info.Level,
            };
        }

        #endregion 私有方法简化
    }
}