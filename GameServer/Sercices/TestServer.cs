using ITXCM;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GameServer.Sercices
{
    internal class TestServer : Singleton<TestServer>
    {
        public void Init()
        {

        }
        public TestServer()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<TestRequestProto>(RecvTestRequest);
        }

        private void RecvTestRequest(NetConnection<NetSession> sender, TestRequestProto message)
        {
            Log.InfoFormat("TestRequestProto:{0}", message.testContent);
            SendTestResponse(sender);
        }
        private void SendTestResponse(NetConnection<NetSession> client)
        {
            client.Session.Response.testResponseProto = new TestResponseProto();
            TestResponseProto msg = client.Session.Response.testResponseProto;
            msg.Res = "This message is send by Server";
            client.SendResponse();
        }
    }
}
