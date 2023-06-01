using ITXCM.Client.Models;
using SkillBridge.Message;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace ITXCM.Client.Services
{
    public class UserService : Singleton<UserService>, IDisposable
    {
        public void Init()
        { }

        public UserService()
        {
            MessageDistributer.Instance.Subscribe<UserLoginResponse>(OnLogin);
            MessageDistributer.Instance.Subscribe<UserRegisterResponse>(OnRegister);
            MessageDistributer.Instance.Subscribe<UserCreateCharacterResponse>(OnCreateCharacter);
            MessageDistributer.Instance.Subscribe<UserGameEnterResponse>(OnGameEnter);
            MessageDistributer.Instance.Subscribe<UserGameLeaveResponse>(OnGameLeave);
        }

        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<UserLoginResponse>(OnLogin);
            MessageDistributer.Instance.Unsubscribe<UserRegisterResponse>(OnRegister);
            MessageDistributer.Instance.Unsubscribe<UserCreateCharacterResponse>(OnCreateCharacter);
            MessageDistributer.Instance.Unsubscribe<UserGameEnterResponse>(OnGameEnter);
            MessageDistributer.Instance.Unsubscribe<UserGameLeaveResponse>(OnGameLeave);
        }

        #region 登录

        public UnityAction<Result, string> LoginCall;

        public void Login(string username, string password)
        {
            Debug.Log($"Login: [ Username:{username} Passward:{password} ] ");
            NetMessage msg = new NetMessage { Request = new NetMessageRequest() };
            msg.Request.userLogin = new UserLoginRequest { User = username, Passward = password };
            NetService.Instance.CheckConnentAndSend(msg);
        }

        private void OnLogin(object sender, UserLoginResponse res)
        {
            Debug.LogFormat($"OnLogin: [ Result:{res.Result} Errormsg:{res.Errormsg} ] ");
            if (res.Result == Result.Success) User.Instance.SetupUserInfo(res.Userinfo);
            LoginCall?.Invoke(res.Result, res.Errormsg);
        }

        #endregion 登录

        #region 注册

        public UnityAction<Result, string> RegisterCall;

        public void Register(string username, string password)
        {
            Debug.Log($"Register: [ username:{username} password:{password} ] ");
            NetMessage msg = new NetMessage { Request = new NetMessageRequest() };
            msg.Request.userRegister = new UserRegisterRequest { User = username, Passward = password };
            NetService.Instance.CheckConnentAndSend(msg);
        }

        private void OnRegister(object sender, UserRegisterResponse res)
        {
            Debug.LogFormat($"OnRegister: [ Result:{res.Result} Errormsg:{res.Errormsg} ] ");
            RegisterCall?.Invoke(res.Result, res.Errormsg);
        }

        #endregion 注册

        #region 创建角色

        public UnityAction<Result, string> CreateCharacterCall;

        public void CreateCharacter(string characterName, CharacterClass cls)
        {
            Debug.Log($"CreateCharacter: [ CharacterName:{characterName} CharacterClass:{cls} ] ");
            NetMessage msg = new NetMessage { Request = new NetMessageRequest() };
            msg.Request.createChar = new UserCreateCharacterRequest { Name = characterName, Class = cls };
            NetService.Instance.CheckConnentAndSend(msg);
        }

        private void OnCreateCharacter(object sender, UserCreateCharacterResponse res)
        {
            Debug.LogFormat($"OnCreateCharacter: [ Result:{res.Result} Errormsg:{res.Errormsg} ] ");
            if (res.Result == Result.Success) User.Instance.SetupCharacterListInfo(res.Characters);
            CreateCharacterCall?.Invoke(res.Result, res.Errormsg);
        }

        #endregion 创建角色

        #region 进入游戏

        public UnityAction<Result, string> GameEnterCall;

        public void GameEnter(int characterIndex)
        {
            Debug.Log($"GameEnter: [ CharacterIndex:{characterIndex} ] ");
            NetMessage msg = new NetMessage { Request = new NetMessageRequest() };
            msg.Request.gameEnter = new UserGameEnterRequest { characterIdx = characterIndex };
            NetService.Instance.CheckConnentAndSend(msg);
        }

        private void OnGameEnter(object sender, UserGameEnterResponse res)
        {
            Debug.LogFormat($"OnGameEnter: [ Result:{res.Result} Errormsg:{res.Errormsg} ] ");
            if (res.Result == Result.Success && res.Character != null)
            {
                User.Instance.SetCharacterInfo(res.Character);

                // TODO 初始化其他管理系统
                /*ItemManager.Instance.Init(res.Character.Items);
                BagManager.Instance.Init(res.Character.Bag);
                EquipManager.Instance.Init(res.Character.Equips);
                QuestManager.Instance.Init(res.Character.Quests);
                FriendManager.Instance.Init(res.Character.Friends);
                GuildManager.Instance.Init(res.Character.Guild);*/
            };
            GameEnterCall?.Invoke(res.Result, res.Errormsg);
        }

        #endregion 进入游戏

        #region 离开游戏

        private bool isQuitGame = false;

        public UnityAction<Result, string> GameLeaveCall;

        public void GameLeave(bool isQuitGame = false)
        {
            this.isQuitGame = isQuitGame;
            Debug.Log($"GameLeave: [ isQuitGame:{isQuitGame} ] ");
            NetMessage msg = new NetMessage { Request = new NetMessageRequest() };
            msg.Request.gameLeave = new UserGameLeaveRequest();
            NetService.Instance.CheckConnentAndSend(msg);
        }

        private void OnGameLeave(object sender, UserGameLeaveResponse res)
        {
            Debug.LogFormat($"OnGameLeave: [ Result:{res.Result} Errormsg:{res.Errormsg} ] ");

            if (isQuitGame)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }

            if (res.Result == Result.Success && User.Instance.CharacterInfo != null)
            {
                User.Instance.SetCharacterInfo(null);
                //  MapService.Instance.CurrentMapId = 0;
            }
            GameLeaveCall?.Invoke(res.Result, res.Errormsg);
        }

        #endregion 离开游戏
    }
}