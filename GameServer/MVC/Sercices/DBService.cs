using ITXCM;

namespace GameServer.Services
{
    /*

     对于添加数据可以用地址指针去操作保存 : 即直接修改TCharacter的数据 然后保存
     对于删除数据只能通过操作数据库保存 : 即只能DBService.Instance.Entities.CharacterFriends.Remove();

     */

    internal class DBService : Singleton<DBService>
    {
        private EntitiesDataModelContainer entities; // 访问数据库的实体

        public EntitiesDataModelContainer Entities => entities;

        public void Init() => entities = new EntitiesDataModelContainer();

        public void Save(bool async = false)
        {
            if (async) entities.SaveChangesAsync();
            else entities.SaveChanges();
        }
    }
}