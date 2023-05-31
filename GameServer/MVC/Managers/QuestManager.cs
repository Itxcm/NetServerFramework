using GameServer.Entities;
using GameServer.Services;
using ITXCM;
using ITXCM.Data;
using SkillBridge.Message;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Managers
{
    public class QuestManager : Singleton<QuestManager>
    {
        private Character Owner;

        public QuestManager()
        {
        }

        public QuestManager(Character owner)
        {
            Owner = owner;
        }

        #region 客户端请求

        // 接受任务
        public QuestAcceptResponse AcceptQuest(Character cha, int questId)
        {
            QuestAcceptResponse msg = new QuestAcceptResponse();

            if (DataManager.Instance.Quests.TryGetValue(questId, out QuestDefine quest))
            {
                var dbQuest = DBService.Instance.Entities.CharacterQuests.Create(); // 创建一个新任务
                dbQuest.QuestID = quest.ID;
                if (quest.Target1 == QuestTarget.None) dbQuest.Status = (int)QuestStatus.Complated; // 没有目标直接完成
                else dbQuest.Status = (int)QuestStatus.InProgress; // 有目标

                msg.Quest = GetQuestInfo(dbQuest); // 将 生成的数据库信息 转成 传输信息 赋值
                msg.Result = Result.Success;
                msg.Errormsg = "接收任务成功!";

                cha.Data.Quests.Add(dbQuest); // 添加到缓存会话
                DBService.Instance.Save();
                return msg;
            }
            else
            {
                msg.Result = Result.Failed;
                msg.Errormsg = "非法任务id!";
                return msg;
            }
        }

        // 提交任务
        public QuestSubmitResponse SubmitQuest(Character cha, int questId)
        {
            QuestSubmitResponse msg = new QuestSubmitResponse();
            if (DataManager.Instance.Quests.TryGetValue(questId, out QuestDefine quest))
            {
                var dbQuest = cha.Data.Quests.Where(q => q.QuestID == questId).FirstOrDefault(); // 查询缓存
                if (dbQuest != null)
                {
                    if (dbQuest.Status != (int)QuestStatus.Complated)
                    {
                        msg.Result = Result.Failed;
                        msg.Errormsg = "任务未完成!";
                        return msg;
                    }
                    // 完成了
                    dbQuest.Status = (int)QuestStatus.Finished; // 更新缓存中的状态

                    msg.Quest = GetQuestInfo(dbQuest);
                    msg.Result = Result.Success;
                    msg.Errormsg = "任务完成!";

                    DBService.Instance.Save(); // 同步缓存中的任务状态

                    // 处理奖励
                    if (quest.RewardGold > 0) cha.Gold += quest.RewardGold;
                    if (quest.RewardExp > 0) { };

                    // 添加缓存
                    if (quest.RewardItem1 > 0) cha.ItemManager.AddItem(quest.RewardItem1, quest.RewardItem1Count);
                    if (quest.RewardItem2 > 0) cha.ItemManager.AddItem(quest.RewardItem2, quest.RewardItem2Count);
                    if (quest.RewardItem3 > 0) cha.ItemManager.AddItem(quest.RewardItem3, quest.RewardItem3Count);

                    DBService.Instance.Save(); // 同步缓存中的任务奖励
                    return msg;
                }

                // 缓存中不在 或数据库中不存在
                msg.Result = Result.Failed;
                msg.Errormsg = "任务未能同步 请重新登录!";
                return msg;
            }
            else
            {
                msg.Result = Result.Failed;
                msg.Errormsg = "非法任务id";
                return msg;
            }
        }

        // 将数据库存储的任务信息赋值给角色

        public void GetQuestInfos(List<NQuestInfo> list)
        {
            foreach (var dbQuest in Owner.Data.Quests) list.Add(GetQuestInfo(dbQuest));
        }

        // 将数据表信息转成 网络传输信息
        public NQuestInfo GetQuestInfo(TCharacterQuest dbQuest)
        {
            NQuestInfo info = new NQuestInfo
            {
                QuestId = dbQuest.QuestID,
                QuestGuid = dbQuest.Id,
                Status = (QuestStatus)dbQuest.Status,
                Targets = new int[3]
                  {
                        dbQuest.Target1,
                        dbQuest.Target2,
                        dbQuest.Target3,
                  }
            };
            return info;
        }

        #endregion 客户端请求
    }
}