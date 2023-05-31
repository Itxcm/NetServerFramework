using ITXCM.Construct;
using SkillBridge.Message;

namespace GameServer.Entities
{
    public class Entity
    {
        public Entity(Vector3Int pos, Vector3Int dir) => EntityData = new NEntity { Position = pos, Direction = dir };
        public Entity(NEntity entity) => entityData = entity;

        public int EntityId => EntityData.Id; // 实体id

        #region 实体传输数据

        private NEntity entityData;

        public NEntity EntityData
        {
            get => entityData;
            set
            {
                entityData = value;
                SetEntityData(value);
            }
        }
        #endregion 实体传输数据

        #region 实体位置

        private Vector3Int position;

        public Vector3Int Position
        {
            get => position;
            set
            {
                position = value;
                entityData.Position = position;
            }
        }
        #endregion 实体位置

        #region 实体方向

        private Vector3Int direction;
        public Vector3Int Direction
        {
            get => direction;
            set
            {
                direction = value;
                entityData.Direction = direction;
            }
        }

        #endregion 实体方向

        #region 实体速度

        private int speed;

        public int Speed
        {
            get => speed;
            set
            {
                speed = value;
                entityData.Speed = speed;
            }
        }
        #endregion 实体速度

        // 设置实体数据
        public void SetEntityData(NEntity entity)
        {
            Position = entity.Position;
            Direction = entity.Direction;
            speed = entity.Speed;
        }
    }
}