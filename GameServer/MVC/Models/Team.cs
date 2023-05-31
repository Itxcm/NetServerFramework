using GameServer.Entities;
using ITXCM;
using SkillBridge.Message;
using System.Collections.Generic;

namespace GameServer.Models
{
    public class Team
    {
        public int Id; // 队伍id
        public Character Leader; // 队长
        public List<Character> Members = new List<Character>(); // 成员列表

        public double timeStamp; // 时间戳

        public Team(Character leader) => AddMember(leader); // 添加当前创建的人为队长

        // 添加成员
        public void AddMember(Character member)
        {
            if (Members.Count == 0) Leader = member;
            Members.Add(member);
            member.Team = this;
            timeStamp = TimeUtil.timeStamp;
        }

        // 离开
        public void Leave(Character member)
        {
            Log.InfoFormat($"Leave Team : {member.Id}:{member.Info.Name}");
            Members.Remove(member);
            if (member == Leader) // 队长离开
            {
                if (Members.Count > 0) Leader = Members[0]; // 赋值第一个为队长
                else Leader = null;
            }
            member.Team = null;
            timeStamp = TimeUtil.timeStamp;
        }

        // 批处理消息
        public void PostProcess(NetMessageResponse msg)
        {
            if (msg.teamInfo == null)
            {
                msg.teamInfo = new TeamInfoResponse(); // 重新new个协议处理 刷新队伍信息
                msg.teamInfo.Result = Result.Success;
                msg.teamInfo.Team = new NTeamInfo();
                msg.teamInfo.Team.Id = Id;
                msg.teamInfo.Team.Leader = Leader.Id;

                foreach (var member in Members) msg.teamInfo.Team.Members.Add(member.GetBasicInfo()); // 添加更新后的成员
            }
        }
    }
}