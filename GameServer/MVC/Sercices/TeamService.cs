using GameServer.Entities;
using GameServer.Managers;
using GameServer.Network;
using ITXCM;
using SkillBridge.Message;
using System.Collections.Generic;

namespace GameServer.Services
{
    public class TeamService : Singleton<TeamService>
    {
        public void Init() => TeamManager.Instance.Init();

        public TeamService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<TeamInviteRequest>(Recv_TeamInviteRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<TeamInviteResponse>(Recv_TeamInviteResponse);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<TeamLeaveRequest>(Recv_TeamLeaveRequest);
        }

        // 组队邀请
        private void Recv_TeamInviteRequest(NetConnection<NetSession> client, TeamInviteRequest request)
        {
            Log.InfoFormat($"Recv_TeamInviteRequest: FromId:{request.FromId} FromName:{request.FromName} ToId:{request.ToId} ToName:{request.ToName}");

            //TODO:执行前置数据效验

            TeamInviteResponse msg = new TeamInviteResponse();

            // 拉取该玩家的客户端
            NetConnection<NetSession> friendClient = SessionManager.Instance.GetSession(request.ToId);

            // 该玩家掉线了
            if (friendClient == null)
            {
                msg.Result = Result.Failed;
                msg.Errormsg = "邀请的好友下线了";
                client.Session.Response.teamInviteRes = msg;
                client.SendResponse();
                return;
            }
            // 该玩家有队伍了
            if (friendClient.Session.Character.Team != null)
            {
                msg.Result = Result.Failed;
                msg.Errormsg = "对方已经有了队伍";
                client.Session.Response.teamInviteRes = msg;
                client.SendResponse();
                return;
            }
            Log.InfoFormat($"ForwardTeamInviteRequest: FromId:{request.FromId} FromName:{request.FromName} ToId:{request.ToId} ToName:{request.ToName}");
            friendClient.Session.Response.teamInviteReq = request; // 转发请求
            friendClient.SendResponse();
        }

        // 组队邀请响应
        private void Recv_TeamInviteResponse(NetConnection<NetSession> client, TeamInviteResponse request)
        {
            Character character = client.Session.Character; // 当前玩家
            Log.InfoFormat($"Recv_FriendAddResponse: character:{character.Id} Result:{request.Result} FromId:{request.Request.FromId} ToId:{request.Request.ToId}");

            // 拉取请求者的客户端
            var requsterClient = SessionManager.Instance.GetSession(request.Request.FromId);

            if (requsterClient == null)
            {
                request.Result = Result.Failed;
                request.Errormsg = "请求者已下线";

                // 只响应自身
                client.Session.Response.teamInviteRes = request;
                client.SendResponse();
            }
            else
            {
                // 获取该玩家的角色
                Character friendCharacter = requsterClient.Session.Character;

                if (request.Result == Result.Success) // 接受了组队
                {
                    // 自己和该玩家 添加到队伍管理
                    TeamManager.Instance.AddTeamMamber(friendCharacter, character);

                    // 协议
                    request.Result = Result.Success;
                    request.Errormsg = "组队成功";

                    // 都响应
                    requsterClient.Session.Response.teamInviteRes = request;
                    requsterClient.SendResponse();

                    client.Session.Response.teamInviteRes = request;
                    client.SendResponse();

                    // 补充 响应队伍中其他队员
                    var members = client.Session.Character.Team.Members; // 记录下这个队伍的成员
                    PostProcess(members);
                }
                else // 拒绝了 只响应请求者的
                {
                    request.Result = Result.Failed;
                    request.Errormsg = $"玩家{character.Name}拒绝了你的组队邀请";

                    requsterClient.Session.Response.teamInviteRes = request;
                    requsterClient.SendResponse();
                }
            }
        }

        // 队伍离开
        private void Recv_TeamLeaveRequest(NetConnection<NetSession> client, TeamLeaveRequest request)
        {
            Log.InfoFormat($"Recv_TeamLeaveRequest: characterId:{request.characterId} TeamId:{request.TeamId}");

            TeamLeaveResponse msg = new TeamLeaveResponse();

            //  自己的客户端发离开成功
            msg.Result = Result.Success;
            client.Session.Response.teamLeave = msg;
            client.SendResponse();

            // 队伍中删除这个人
            var members = client.Session.Character.Team.Members; // 记录下这个队伍的成员
            TeamManager.Instance.RemoveMember(CharacterManager.Instance.GetCharacter(request.characterId));

            // 批处理更新队伍信息
            PostProcess(members);
        }

        // 批处理更新队伍信息
        private void PostProcess(List<Character> members)
        {
            foreach (var member in members)
            {
                var otherClient = SessionManager.Instance.GetSession(member.Id);

                if (otherClient != null)
                {
                    otherClient.Session.Response.teamInfo = null; // 重置下队伍信息 让其能批处理
                    otherClient.SendResponse();
                }
            }
        }
    }
}