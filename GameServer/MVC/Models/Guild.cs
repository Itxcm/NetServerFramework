using GameServer.Entities;
using GameServer.Managers;
using GameServer.Services;
using ITXCM;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Models
{
    public class Guild
    {
        public TGuild Data; // 工会数据库信息 (该地址修改保存会同步数据库 注:可添加保存 不可删除保存)

        public int Id => Data.Id; // 工会id

        public string Name => Data.Name; // 工会名称

        public double timeStamp; // 更新时间戳

        public Guild(TGuild tGuild) => Data = tGuild;

        // 获取工会数据库中的角色信息
        private TGuildMember GetDBGuildMember(int characterId)
        {
            foreach (var member in Data.Members)
            {
                if (member.CharacterId == characterId) return member;
            }
            return null;
        }

        // 获取工会信息
        public NGuildInfo GetGuildInfo(Character character)
        {
            NGuildInfo info = new NGuildInfo
            {
                Id = Id,
                GuildName = Name,
                Notice = Data.Notice,
                leaderId = Data.LeaderID,
                leaderName = Data.LeaderName,
                createTime = (long)TimeUtil.GetTimestamp(Data.CreateTime),
                memberCount = Data.Members.Count
            };

            if (character != null) // 工会成员可查看额外的详细信息
            {
                info.Members.AddRange(GetMemberInfos()); // 普通成员可查看成员信息

                info.Applies.AddRange(GetApplyInfos()); // 这里直接加了 (到时候看要不要限制下)
            }

            return info;
        }

        // 获取工会成员列表信息

        public List<NGuildMemberInfo> GetMemberInfos()
        {
            List<NGuildMemberInfo> members = new List<NGuildMemberInfo>();

            foreach (var member in Data.Members) // 遍历缓存数据库中数据
            {
                // 转换传输信息
                var memberInfo = new NGuildMemberInfo
                {
                    Id = member.Id,
                    characterId = member.CharacterId,
                    Title = (GuildTitle)member.Title,
                    joinTime = (long)TimeUtil.GetTimestamp(member.JoinTime),
                    lastTime = (long)TimeUtil.GetTimestamp(member.LastTime),
                };

                var character = CharacterManager.Instance.GetCharacter(member.CharacterId);
                if (character != null) // 角色在线 更新数据
                {
                    // 状态赋值
                    memberInfo.Info = character.GetBasicInfo();
                    memberInfo.Status = 1;

                    // 状态更新
                    member.Level = character.Data.Level;
                    member.Name = character.Data.Name;
                    member.LastTime = DateTime.Now;
                }
                else
                {
                    memberInfo.Info = GetMemberInfos(member);
                    memberInfo.Status = 0;
                }

                members.Add(memberInfo); // 添加到列表中
            }

            return members;
        }

        // 数据库公会成员信息 转成 传输信息
        public NCharacterInfo GetMemberInfos(TGuildMember member)
        {
            return new NCharacterInfo
            {
                Id = member.Id,
                Name = member.Name,
                Class = (CharacterClass)member.Class,
                Level = member.Level,
            };
        }

        // 获取审批列表信息
        public List<NGuildApplyInfo> GetApplyInfos()
        {
            List<NGuildApplyInfo> applies = new List<NGuildApplyInfo>();

            foreach (var apply in Data.Applies)
            {
                if (apply.Result != (int)ApplyResult.None) continue; // 其他状态跳过(审批了的和拒绝的)
                NGuildApplyInfo info = new NGuildApplyInfo
                {
                    characterId = apply.CharacterId,
                    GuildId = apply.GuildId,
                    Class = apply.Class,
                    Level = apply.Level,
                    Name = apply.Name,
                    Result = (ApplyResult)apply.Result,
                };
                applies.Add(info);
            }

            return applies;
        }

        // 批处理协议赋值
        public void PostProcess(Character character, NetMessageResponse message)
        {
            if (message.Guild == null)
            {
                message.Guild = new GuildResponse { Result = Result.Success, guildInfo = GetGuildInfo(character) };
            }
        }

        #region 操作

        // 添加工会成员
        public void AddMember(int characterId, string name, int @class, int level, GuildTitle title)
        {
            DateTime now = DateTime.Now;

            // 添加工会成员
            TGuildMember dbMember = new TGuildMember
            {
                CharacterId = characterId,
                Name = name,
                Level = level,
                Class = @class,
                Title = (int)title,
                JoinTime = now,
                LastTime = now,
            };
            Data.Members.Add(dbMember);
            DBService.Instance.Save();

            // 更新角色信息
            UpdateCharacter(characterId, Id, this);
            timeStamp = TimeUtil.timeStamp;
        }

        // 删除工会成员
        public void RemoveMember(int characterId)
        {
            // 删除工会成员
            var member = Data.Members.FirstOrDefault(v => v.CharacterId == characterId);
            DBService.Instance.Entities.GuildMembers.Remove(member);
            DBService.Instance.Save();

            // 更新角色信息
            UpdateCharacter(characterId, 0, null);
            timeStamp = TimeUtil.timeStamp;
        }

        // 添加申请记录
        public void AddApplyRecord(NGuildApplyInfo applyInfo)
        {
            // 创建申请记录
            var dbApply = DBService.Instance.Entities.GuildApplies.Create();
            dbApply.GuildId = applyInfo.GuildId;
            dbApply.CharacterId = applyInfo.characterId;
            dbApply.Name = applyInfo.Name;
            dbApply.Class = applyInfo.Class;
            dbApply.Level = applyInfo.Level;
            dbApply.Result = (int)ApplyResult.None;
            dbApply.ApplyTime = DateTime.Now;

            // 保存
            Data.Applies.Add(dbApply);
            DBService.Instance.Save();
        }

        // 删除申请记录
        public void RemoveApplyRecord(int characterId)
        {
            var dbApplyInfo = DBService.Instance.Entities.GuildApplies.FirstOrDefault(v => v.CharacterId == characterId);
            if (dbApplyInfo != null)
            {
                DBService.Instance.Entities.GuildApplies.Remove(dbApplyInfo);
                DBService.Instance.Save();
            }
        }

        // 申请检测
        public bool JoinApplyCheck(NGuildApplyInfo applyInfo)
        {
            // 查下有没有这个申请
            var oldApply = Data.Applies.FirstOrDefault(v => v.CharacterId == applyInfo.characterId);

            if (oldApply == null)
            {
                // 没有记录 直接加
                AddApplyRecord(applyInfo);
                timeStamp = TimeUtil.timeStamp;
                return true;
            }
            else if (oldApply.Result == (int)ApplyResult.Reject) // 只有被拒绝了才修改
            {
                oldApply.Result = (int)applyInfo.Result;
                DBService.Instance.Save();
                timeStamp = TimeUtil.timeStamp;
                return true;
            }

            return false;
        }

        // 申请审批
        public bool JoinApplyApproval(NGuildApplyInfo applyInfo, TGuildApply oldApply)
        {
            // 修改申请结果
            oldApply.Result = (int)applyInfo.Result;
            DBService.Instance.Save();
            timeStamp = TimeUtil.timeStamp;

            // 通过
            if (applyInfo.Result == ApplyResult.Accept)
            {
                AddMember(applyInfo.characterId, applyInfo.Name, applyInfo.Class, applyInfo.Level, GuildTitle.None);
                return true;
            }
            return false;
        }

        // 执行工会命令
        public void ExcuteAdmin(GuildAdminCommand command, int targetId, int sourceId)
        {
            TGuildMember target = GetDBGuildMember(targetId);
            TGuildMember source = GetDBGuildMember(sourceId);

            switch (command)
            {
                case GuildAdminCommand.Kickout:
                    var client = SessionManager.Instance.GetSession(targetId);

                    if (client == null) // 不在线
                    {
                        // 数据库中移除
                        var tCha = DBService.Instance.Entities.Characters.FirstOrDefault(v => v.ID == targetId);
                        var locaCha = CharacterManager.Instance.GetCharacter(tCha);
                        Leave(locaCha);
                    }
                    else
                    {
                        var cha = client.Session.Character;
                        var guild = cha.Guild;
                        guild.Leave(cha);

                        // 发送更新工会
                        client.Session.Response.Guild = new GuildResponse { Result = Result.Success, guildInfo = null };
                        client.SendResponse();
                    }
                    break;

                case GuildAdminCommand.Promote:
                    target.Title = (int)GuildTitle.VicePresident;
                    break;

                case GuildAdminCommand.Depost:
                    target.Title = (int)GuildTitle.None;
                    break;

                case GuildAdminCommand.Transfer:
                    target.Title = (int)GuildTitle.President;
                    source.Title = (int)GuildTitle.None;
                    Data.LeaderID = target.CharacterId;
                    Data.LeaderName = target.Name;
                    break;

                default:
                    break;
            }
            DBService.Instance.Save();
            timeStamp = TimeUtil.timeStamp;
        }

        // 离开工会
        public void Leave(Character character)
        {
            var info = GetGuildInfo(character);

            //TODO待补充 离开工会要判断删审批
            if (Data.LeaderID == character.Id)  // 离开的是会长
            {
                if (info.memberCount > 0) // 还有人在 自动继承
                {
                    for (var i = 0; i < info.Members.Count; i++)
                    {
                        if (info.Members[i].Title == GuildTitle.President) continue; // 是会长跳过
                        var targetId = info.Members[i].characterId;
                        ExcuteAdmin(GuildAdminCommand.Transfer, targetId, character.Id); // 执行转让
                        break;
                    }
                }
                else
                {
                    foreach (var applyInfo in GetApplyInfos()) RemoveApplyRecord(applyInfo.characterId); // 移除所有申请记录
                    GuildManager.Instance.DeleteGuild(info.Id); // 移除工会
                }
            }
            // 移除这个人和他的记录
            RemoveMember(character.Id);
            RemoveApplyRecord(character.Id);
        }

        // 向当前在线成员发送同步
        public void SendToOnlineMember()
        {
            foreach (var member in GetMemberInfos())
            {
                var otherClient = SessionManager.Instance.GetSession(member.characterId);
                if (otherClient != null)
                {
                    otherClient.Session.Response.Guild = null;
                    otherClient.SendResponse();
                }
            }
        }

        #endregion 操作

        #region 方法抽离

        // 更新角色工会信息
        public void UpdateCharacter(int characterId, int newGuildId, Guild guild)
        {
            var character = CharacterManager.Instance.GetCharacter(characterId);

            if (character != null)
            {
                character.Data.GuildId = newGuildId;
                character.Guild = guild;
            }
            else
            {
                TCharacter dbCharacter = DBService.Instance.Entities.Characters.SingleOrDefault(c => c.ID == characterId);
                dbCharacter.GuildId = newGuildId;
            }
            DBService.Instance.Save();
        }

        #endregion 方法抽离
    }
}