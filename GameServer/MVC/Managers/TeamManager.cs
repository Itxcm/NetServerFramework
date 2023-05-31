using GameServer.Entities;
using GameServer.Models;
using ITXCM;
using System.Collections.Generic;

namespace GameServer.Managers
{
    internal class TeamManager : Singleton<TeamManager>
    {
        // 都是表示当前队伍

        public List<Team> Teams = new List<Team>();
        public Dictionary<int, Team> CharacterTeams = new Dictionary<int, Team>(); // 队伍id - 队伍

        public void Init()
        { }

        // 获取角色当前队伍
        public Team GetTeamByCharacterId(int characterId)
        {
            CharacterTeams.TryGetValue(characterId, out Team team);
            return team;
        }

        // 添加队伍成员
        public void AddTeamMamber(Character leader, Character member)
        {
            if (leader.Team == null) leader.Team = CreateTeam(leader); // 没有队伍则创建个新的

            leader.Team.AddMember(member); // 加到队长的队伍
        }

        // 移除队员
        public void RemoveMember(Character member)
        {
            member.Team.Leave(member);
        }

        // 创建队伍
        private Team CreateTeam(Character leader)
        {
            Team team;

            // 遍历当前存在的所有队伍
            for (int i = 0; i < Teams.Count; i++)
            {
                team = Teams[i];
                if (team.Members.Count == 0) //查找成员为空的队伍
                {
                    team.AddMember(leader); // 启用这个队伍
                    return team;
                }
            }

            // 当前存在的队伍中没有空的
            team = new Team(leader);
            team.Id = Teams.Count; // 把当前的列表长度赋值为id
            Teams.Add(team); // 添加到列表中
            CharacterTeams.Add(team.Id, team); // 添加到字典
            return team;
        }
    }
}