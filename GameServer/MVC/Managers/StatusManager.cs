using GameServer.Entities;
using SkillBridge.Message;
using System.Collections.Generic;

namespace GameServer.Managers
{
    public class StatusManager
    {
        public Character Owner;
        private List<NStatus> Status { get; set; } // 状态管理列表

        public bool HasStatus => Status.Count > 0;

        public StatusManager(Character owner)
        {
            Owner = owner;
            Status = new List<NStatus>();
        }

        // 添加状态
        public void AddStatus(StatusType type, int id, int value, StatusAction action)
        {
            Status.Add(new NStatus
            {
                Type = type,
                Id = id,
                Value = value,
                Action = action
            });
        }

        // 添加金币改变
        public void AddGoldChange(int count)
        {
            if (count > 0) AddStatus(StatusType.Money, 0, count, StatusAction.Add);
            else AddStatus(StatusType.Money, 0, -count, StatusAction.Delete);
        }

        // 添加道具变化
        public void AddItemChange(int id, int count, StatusAction action) => AddStatus(StatusType.Item, id, count, action);

        // 将状态管理列表添加批处理
        public void PostProcess(NetMessageResponse message)
        {
            if (message.statusNotify == null) message.statusNotify = new StatusNotify();
            foreach (NStatus status in Status) message.statusNotify.Status.Add(status);
            Status.Clear();
        }
    }
}