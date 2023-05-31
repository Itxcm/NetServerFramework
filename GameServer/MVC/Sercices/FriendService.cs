using GameServer.Entities;
using GameServer.Managers;
using GameServer.Network;
using ITXCM;
using SkillBridge.Message;
using System.Linq;

namespace GameServer.Services
{
    public class FriendService : Singleton<FriendService>
    {
        public FriendService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FriendAddRequest>(Recv_FriendAddRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FriendAddResponse>(Recv_FriendAddResponse);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FriendRemoveRequest>(Recv_FriendRemoveRequest);
        }

        // 收到加好友请求((玩家发送请求))
        private void Recv_FriendAddRequest(NetConnection<NetSession> client, FriendAddRequest request)
        {
            Log.InfoFormat($"Recv_FriendAddRequest: FromId:{request.FromId} FromName:{request.FromName} ToId:{request.ToId} ToName:{request.ToName}");

            Character character = client.Session.Character;
            FriendAddResponse msg = new FriendAddResponse();

            if (request.ToId == 0) // 没有传入id
            {
                foreach (var cha in CharacterManager.Instance.Characters) // 查找姓名字段 在缓存中(在线玩家)
                {
                    if (cha.Value.Data.Name == request.ToName)
                    {
                        request.ToId = cha.Key;
                        break;
                    }
                }
            }
            NetConnection<NetSession> friend = null;
            if (request.ToId > 0) // 该玩家存在
            {
                if (character.FriendManager.GetFriendByFriendId(request.ToId) != null) // 在自己的好友列表中查一下
                {
                    msg.Result = Result.Failed;
                    msg.Errormsg = "已经是好友了";
                    client.Session.Response.friendAddRes = msg;
                    client.SendResponse();
                    return;
                }
                friend = SessionManager.Instance.GetSession(request.ToId); // 拉取该玩家的客户端
            }
            if (friend == null) // 该玩家客户端不存在(掉线了或未上线)
            {
                msg.Result = Result.Failed;
                msg.Errormsg = "玩家不存在或者不在线";
                client.Session.Response.friendAddRes = msg;
                client.SendResponse();
                return;
            }
            Log.InfoFormat($"ForwardRequest: FromId:{request.FromId} FromName:{request.FromName} ToId:{request.ToId} ToName:{request.ToName}");
            friend.Session.Response.friendAddReq = request; // 直接将玩家的请求协议发送给好友
            friend.SendResponse();
        }

        // 收到加好友的响应(玩家处理请求)
        private void Recv_FriendAddResponse(NetConnection<NetSession> client, FriendAddResponse request)
        {
            Character character = client.Session.Character; // 当前玩家
            Log.InfoFormat($"Recv_FriendAddResponse: character:{character.Id} Result:{request.Result} FromId:{request.Request.FromId} ToId:{request.Request.ToId}");

            var requsterClient = SessionManager.Instance.GetSession(request.Request.FromId); // 拉取请求者的客户端

            if (requsterClient == null)
            {
                request.Result = Result.Failed;
                request.Errormsg = "请求者已下线";

                // 只响应自身
                client.Session.Response.friendAddRes = request;
                client.SendResponse();
            }
            else
            {
                // 获取好友的角色
                Character friendCharacter = requsterClient.Session.Character;

                if (request.Result == Result.Success) // 接受了好友请求
                {
                    // 互相加好友 并保存
                    character.FriendManager.AddFriend(friendCharacter);
                    friendCharacter.FriendManager.AddFriend(character);
                    DBService.Instance.Save();

                    // 协议
                    request.Result = Result.Success;
                    request.Errormsg = "添加好友成功";

                    // 都响应
                    requsterClient.Session.Response.friendAddRes = request;
                    requsterClient.SendResponse();

                    client.Session.Response.friendAddRes = request;
                    client.SendResponse();
                }
                else // 拒绝了 只响应请求者的
                {
                    request.Result = Result.Failed;
                    request.Errormsg = $"玩家{character.Name}拒绝了你的请求";

                    requsterClient.Session.Response.friendAddRes = request;
                    requsterClient.SendResponse();
                }
            }
        }

        // 收到删除好友请求
        private void Recv_FriendRemoveRequest(NetConnection<NetSession> client, FriendRemoveRequest request)
        {
            Log.InfoFormat($"Recv_FriendRemoveRequest: characterId:{request.Id} friendId:{request.friendId}");

            Character character = client.Session.Character;
            FriendRemoveResponse msg = new FriendRemoveResponse();

            var friendClient = SessionManager.Instance.GetSession(request.friendId); // 拉取该好友连接客户端

            // 删除这个好友的自己
            if (friendClient != null)
            {
                // 好友端缓存移除
                if (friendClient.Session.Character.FriendManager.RemoveFriendByFriendId(character.Id))
                {
                    msg.Result = Result.Success;
                    msg.Errormsg = $"您的好友{character.Name}移除了你哦!";
                }
                else
                {
                    msg.Result = Result.Failed;
                    msg.Errormsg = $"好友{character.Name}尝试删除你,但失败了!";
                }

                friendClient.Session.Response.friendRemove = msg;
                friendClient.SendResponse();
            }
            else RemoveFriend(request.friendId, character.Id); // 不在线数据库移除 (后续添加邮件系统通知)

            // 自己移除该好友
            if (character.FriendManager.RemoveFriendById(request.Id))
            {
                msg.Result = Result.Success;
                msg.Errormsg = "删除成功";
            }
            else
            {
                msg.Result = Result.Failed;
                msg.Errormsg = $"尝试删除好友{character.Name},但失败了!";
            }

            DBService.Instance.Save();
            client.Session.Response.friendRemove = msg;
            client.SendResponse();
        }

        // 移除好友
        private void RemoveFriend(int charId, int friendId)
        {
            var removeItem = DBService.Instance.Entities.CharacterFriends.FirstOrDefault(v => v.CharacterID == charId && v.FriendID == friendId);
            if (removeItem != null) DBService.Instance.Entities.CharacterFriends.Remove(removeItem);
        }

        public void Init()
        {
        }
    }
}