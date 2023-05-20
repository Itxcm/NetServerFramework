using SkillBridge.Message;
using ITXCM;

namespace Network
{
    public class NetSession : INetSession
    {

        public IPostResponser PostResponser { get; set; }

        public void Disconnected()
        {
            PostResponser = null;
        }

        private NetMessage response;

        public NetMessageResponse Response
        {
            get
            {
                if (response == null)
                {
                    response = new NetMessage();
                }
                if (response.Response == null)
                    response.Response = new NetMessageResponse();
                return response.Response;
            }
        }

        public byte[] GetResponse()
        {
            if (response != null)
            {
                if (PostResponser != null) PostResponser.PostProcess(Response);

                byte[] data = PackageHandler.PackMessage(response);
                response = null;
                return data;
            }
            return null;
        }
    }
}