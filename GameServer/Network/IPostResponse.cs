using SkillBridge.Message;

namespace GameServer.Network
{
    public interface IPostResponser
    {
        void PostProcess(NetMessageResponse message);
    }
}