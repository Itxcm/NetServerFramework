using SkillBridge.Message;

namespace Network
{
    public interface IPostResponser
    {
        void PostProcess(NetMessageResponse message);
    }
}