using GameServer.Entities;
using GameServer.Models;
using GameServer.Services;
using ITXCM;
using SkillBridge.Message;
using System;
using System.Collections.Generic;

namespace GameServer.Managers
{
    public class GuildManager : Singleton<GuildManager>
    {
        public Dictionary<int, Guild> Guilds = new Dictionary<int, Guild>(); // 当前所有工会
        private HashSet<string> GuildNames = new HashSet<string>(); // 工会名称存在哈希表中

        public void Init()
        {
            Guilds.Clear();

            foreach (var guild in DBService.Instance.Entities.Guilds) AddGuild(new Guild(guild));
        }

        #region 创建工会相关

        // 检测名称是否存在
        public bool CheckNameExisted(string guildName) => GuildNames.Contains(guildName);

        // 创建工会
        public bool CreateGuild(string guildName, string notice, Character leader)
        {
            DateTime now = DateTime.Now;

            // 创建字段 并保存更新工会的id自增字段
            TGuild dbGuild = DBService.Instance.Entities.Guilds.Create();
            dbGuild.Name = guildName;
            dbGuild.Notice = notice;
            dbGuild.LeaderID = leader.Id;
            dbGuild.LeaderName = leader.Name;
            dbGuild.CreateTime = now;
            DBService.Instance.Entities.Guilds.Add(dbGuild);
            DBService.Instance.Save();

            // 创建工会 添加工会成员
            Guild guild = new Guild(dbGuild);
            guild.AddMember(leader.Id, leader.Name, leader.Data.Class, leader.Data.Level, GuildTitle.President);

            // 加到工会信息中
            AddGuild(guild);

            return true;
        }

        // 记录添加工会
        private void AddGuild(Guild guild)
        {
            Guilds.Add(guild.Id, guild);
            GuildNames.Add(guild.Name);
            guild.timeStamp = TimeUtil.timeStamp;
        }

        #endregion 创建工会相关

        // 移除工会(工会解散)
        public bool DeleteGuild(int guildId)
        {
            return true;
        }

        #region 加入公会请求 加入工会响应 相关

        // 获取工会信息
        public Guild GetGuildById(int guildId)
        {
            if (guildId == 0) return null;
            Guilds.TryGetValue(guildId, out Guild guild);
            return guild;
        }

        #endregion 加入公会请求 加入工会响应 相关

        // 获取工会列表
        public List<NGuildInfo> GetGuildListInfo()
        {
            List<NGuildInfo> list = new List<NGuildInfo>();

            foreach (var info in Guilds.Values) list.Add(info.GetGuildInfo(null));

            return list;
        }
    }
}