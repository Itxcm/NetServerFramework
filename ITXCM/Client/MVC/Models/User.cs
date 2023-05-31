using ITXCM;
using SkillBridge.Message;
using System.Collections.Generic;
using UnityEngine;

namespace ITXCM.Client.Models
{
    internal class User : Singleton<User>
    {
        #region 用户信息

        private NUserInfo userInfo;

        public NUserInfo UserInfo => userInfo; // 用户信息

        public void SetupUserInfo(NUserInfo userInfo) => this.userInfo = userInfo;

        #endregion 用户信息

        #region 角色信息

        private NCharacterInfo characterInfo;

        public NCharacterInfo CharacterInfo => characterInfo; // 当前选取角色

        public void SetCharacterInfo(NCharacterInfo characterInfo) => this.characterInfo = characterInfo;

        public List<NCharacterInfo> CharacterListInfo => userInfo.Player.Characters;  // 角色列表信息

        public void SetupCharacterListInfo(List<NCharacterInfo> characterInfo)
        {
            userInfo.Player.Characters.Clear();
            userInfo.Player.Characters.AddRange(characterInfo);
        }

        #endregion 角色信息

        //  public NTeamInfo CurrentTeam { get; set; } // 当前队伍信息

        //    public MapDefine CurrentMapData { get; set; }    // 当前地图信息

        public GameObject CurrentCharacterObject { get; set; }  // 当前角色游戏对象


        //  添加金币
        public void AddGold(int count) => CharacterInfo.Gold += count;
    }
}