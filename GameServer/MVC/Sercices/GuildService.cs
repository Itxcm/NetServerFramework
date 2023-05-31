using GameServer.Entities;
using GameServer.Managers;
using GameServer.Network;
using ITXCM;
using SkillBridge.Message;
using System.Linq;

namespace GameServer.Services
{
    public class GuildService : Singleton<GuildService>
    {
        public GuildService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildCreateRequest>(Recv_GuildCreateRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildListRequest>(Recv_GuildListRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildJoinRequest>(Recv_GuildJoinRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildJoinResponse>(Recv_GuildJoinResponse);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildLeaveRequest>(Recv_GuildLeaveRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildAdminRequest>(Recv_GuildAdminRequest);
        }

        // 创建工会请求
        private void Recv_GuildCreateRequest(NetConnection<NetSession> client, GuildCreateRequest request)
        {
            Character character = client.Session.Character;
            Log.InfoFormat($"Recv_GuildCreateRequest: character:{character.Id}-{character.Name} GuildName:{request.GuildName}");

            GuildCreateResponse msg = new GuildCreateResponse();
            client.Session.Response.guildCreate = msg;

            if (character.Guild != null)
            {
                msg.Result = Result.Failed;
                msg.Errormsg = "已经有了工会,无法创建!";
                client.SendResponse();
                return;
            }
            if (GuildManager.Instance.CheckNameExisted(request.GuildName))
            {
                msg.Result = Result.Failed;
                msg.Errormsg = "工会名称已存在!";
                client.SendResponse();
                return;
            }
            GuildManager.Instance.CreateGuild(request.GuildName, request.GuildNotice, character);
            msg.Result = Result.Success;
            msg.guildInfo = character.Guild.GetGuildInfo(character);
            client.SendResponse();
        }

        // 加入工会请求
        private void Recv_GuildJoinRequest(NetConnection<NetSession> client, GuildJoinRequest request)
        {
            Character character = client.Session.Character;
            Log.InfoFormat($"Recv_GuildJoinRequest: character:{character.Id}-{character.Name} GuildId:{request.Apply.GuildId}");

            GuildJoinResponse msg = new GuildJoinResponse();
            client.Session.Response.guildJoinRes = msg;

            var guild = GuildManager.Instance.GetGuildById(request.Apply.GuildId);

            if (guild == null)
            {
                msg.Result = Result.Failed;
                msg.Errormsg = "工会不存在";
                client.SendResponse();
                return;
            }

            // 申请赋值
            request.Apply.characterId = character.Data.ID;
            request.Apply.Name = character.Data.Name;
            request.Apply.Class = character.Data.Class;
            request.Apply.Level = character.Data.Level;

            // 申请检测 (检测这个请求被审批没)
            if (guild.JoinApplyCheck(request.Apply))
            {
                var leaderClient = SessionManager.Instance.GetSession(guild.Data.LeaderID);

                // 会长在线直接转发
                if (leaderClient != null)
                {
                    leaderClient.Session.Response.guildJoinReq = request;
                    leaderClient.SendResponse();
                }
                else
                {
                    // 会长不在线 清除未发送的协议(不清楚将会在下一次批处理发送空的协议)
                    client.Session.Response.guildJoinRes = null;
                }
            }
            else
            {
                msg.Result = Result.Failed;
                msg.Errormsg = "请勿重复申请";
                client.SendResponse();
            }
        }

        // 工会操作响应
        private void Recv_GuildAdminRequest(NetConnection<NetSession> client, GuildAdminRequest request)
        {
            Character character = client.Session.Character;
            Log.InfoFormat($"Recv_GuildAdminRequest: character:{character.Id}-{character.Name} targetID{request.Target}");

            GuildAdminResponse msg = new GuildAdminResponse();
            client.Session.Response.guildAdmin = msg;

            if (character.Guild == null)
            {
                msg.Result = Result.Failed;
                msg.Errormsg = "该角色没有工会,非法操作!";
                client.SendResponse();
                return;
            }
            character.Guild.ExcuteAdmin(request.Command, request.Target, character.Id);

            string targetName; //
            string sourceName = character.Name;

            // 目标通知
            var targetClient = SessionManager.Instance.GetSession(request.Target);
            if (targetClient != null)       // 目标在线
            {
                targetName = CharacterManager.Instance.GetCharacter(request.Target).Name;

                msg.Result = Result.Success;
                msg.Command = request;

                // 判断
                switch (request.Command)
                {
                    case GuildAdminCommand.Kickout:
                        msg.Errormsg = $"你已被{sourceName}踢出工会!";
                        break;

                    case GuildAdminCommand.Promote:
                        msg.Errormsg = $"{sourceName}将你晋升为副会长!";
                        break;

                    case GuildAdminCommand.Depost:
                        msg.Errormsg = $"{sourceName}将你罢免为普通成员!";
                        break;

                    case GuildAdminCommand.Transfer:
                        msg.Errormsg = $"{sourceName}将会长转让给了你!";
                        break;

                    default:
                        break;
                }

                targetClient.Session.Response.guildAdmin = msg;
                targetClient.SendResponse();
            }
            else
            {
                //TODO 邮件通知
                targetName = DBService.Instance.Entities.Characters.FirstOrDefault(v => v.ID == request.Target).Name;
            }

            // 自己通知
            switch (request.Command)
            {
                case GuildAdminCommand.Kickout:
                    msg.Errormsg = $"你已将{targetName}踢出工会!";
                    break;

                case GuildAdminCommand.Promote:
                    msg.Errormsg = $"你已将{targetName}晋升为副会长!";
                    break;

                case GuildAdminCommand.Depost:
                    msg.Errormsg = $"你已将{targetName}罢免为普通成员!";
                    break;

                case GuildAdminCommand.Transfer:
                    msg.Errormsg = $"你已将会长转让给了{targetName}!";
                    break;

                default:
                    break;
            }

            msg.Result = Result.Success;
            msg.Command = request;
            client.SendResponse();
        }

        // 工会列表请求
        private void Recv_GuildListRequest(NetConnection<NetSession> client, GuildListRequest request)
        {
            Character character = client.Session.Character;
            Log.InfoFormat($"Recv_GuildListRequest: character:{character.Id}-{character.Name}");

            GuildListResponse msg = new GuildListResponse();
            client.Session.Response.guildList = msg;

            msg.Guilds.AddRange(GuildManager.Instance.GetGuildListInfo());
            msg.Result = Result.Success;
            client.SendResponse();
        }

        // 加入工会响应请求
        private void Recv_GuildJoinResponse(NetConnection<NetSession> client, GuildJoinResponse request)
        {
            Character character = client.Session.Character;
            Log.InfoFormat($"Recv_GuildJoinResponse: character:{character.Id}-{character.Name} GuildId:{request.Apply.GuildId}");

            GuildJoinResponse msg = new GuildJoinResponse();

            var guild = GuildManager.Instance.GetGuildById(request.Apply.GuildId);

            // 判断是否已经加入了
            var apply = guild.Data.Applies.FirstOrDefault(v => v.CharacterId == request.Apply.characterId && v.Result == 0);
            if (apply == null) return;

            // 判断审批结果
            bool isPass = guild.JoinApplyApproval(request.Apply, apply);

            // 拉取请求者的客户端发送消息
            var requestClient = SessionManager.Instance.GetSession(request.Apply.characterId);

            if (requestClient != null) // 在线
            {
                if (isPass)
                {
                    requestClient.Session.Character.Guild = guild;

                    msg.Result = Result.Success;
                    msg.Errormsg = "加入工会成功";
                    requestClient.Session.Response.guildJoinRes = msg;
                    requestClient.SendResponse();
                }
                else
                {
                    msg.Result = Result.Failed;
                    msg.Errormsg = $"{character.Name}拒绝你加入工会!";
                    requestClient.Session.Response.guildJoinRes = msg;
                    requestClient.SendResponse();
                }
            }
            else
            {
                //TODO 请求者下线了 邮件通知
            }

            // 当前工会在线成员发送批处理更新信息
            guild.SendToOnlineMember();
        }

        // 离开工会响应
        private void Recv_GuildLeaveRequest(NetConnection<NetSession> client, GuildLeaveRequest request)
        {
            Character character = client.Session.Character;
            Log.InfoFormat($"Recv_GuildLeaveRequest: character:{character.Id}-{character.Name}");

            GuildLeaveResponse msg = new GuildLeaveResponse();
            client.Session.Response.guildLeave = msg;

            var guild = character.Guild;
            character.Guild.Leave(character);
            guild.SendToOnlineMember();   // 当前工会在线成员发送批处理更新信息

            msg.Result = Result.Success;
            msg.Errormsg = "离开工会成功!";

            client.SendResponse();
        }

        public void Init() => GuildManager.Instance.Init();
    }
}