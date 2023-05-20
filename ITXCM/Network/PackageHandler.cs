using System;
using System.IO;

namespace ITXCM
{
    /// <summary>
    /// PackageHandler
    /// 数据包处理器
    /// </summary>
    public class PackageHandler : PackageHandler<object>
    {
        public PackageHandler(object sender) : base(sender)
        {
        }
    }

    /// <summary>
    /// PackageHandler
    /// 数据包处理器
    /// </summary>
    /// <typeparam name="T">消息发送者类型</typeparam>
    public class PackageHandler<T>
    {
        private MemoryStream stream = new MemoryStream(64 * 1024);
        private int readOffset = 0;

        private T sender;

        public PackageHandler(T sender)
        {
            this.sender = sender;
        }

        /// <summary>
        /// 接收数据到PackageHandler
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void ReceiveData(byte[] data, int offset, int count)
        {
            if (stream.Position + count > stream.Capacity)
            {
                throw new Exception("PackageHandler write buffer overflow");
            }
            stream.Write(data, offset, count);

            ParsePackage();
        }

        /// <summary>
        /// 打包消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static byte[] PackMessage(SkillBridge.Message.NetMessage message)
        {
            byte[] package = null;
            using (MemoryStream ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, message);
                package = new byte[ms.Length + 4];
                Buffer.BlockCopy(BitConverter.GetBytes(ms.Length), 0, package, 0, 4);
                Buffer.BlockCopy(ms.GetBuffer(), 0, package, 4, (int)ms.Length);
            }
            return package;
        }

        /// <summary>
        /// 提取消息
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static SkillBridge.Message.NetMessage UnpackMessage(byte[] packet, int offset, int length)
        {
            SkillBridge.Message.NetMessage message = null;
            using (MemoryStream ms = new MemoryStream(packet, offset, length))
            {
                message = ProtoBuf.Serializer.Deserialize<SkillBridge.Message.NetMessage>(ms);
            }
            return message;
        }

        /// <summary>
        /// 数据包解析
        /// </summary>
        /// <returns></returns>
        bool ParsePackage()
        {
            if (readOffset + 4 < stream.Position)
            {
                int packageSize = BitConverter.ToInt32(stream.GetBuffer(), readOffset);
                if (packageSize + readOffset + 4 <= stream.Position)
                {//包有效

                    SkillBridge.Message.NetMessage message = UnpackMessage(stream.GetBuffer(), this.readOffset + 4, packageSize);
                    if (message == null)
                    {
                        throw new Exception("PackageHandler ParsePackage faild,invalid package");
                    }
                    MessageDistributer<T>.Instance.ReceiveMessage(this.sender, message);
                    this.readOffset += (packageSize + 4);
                    return ParsePackage();
                }
            }

            //未接收完/要结束了
            if (this.readOffset > 0)
            {
                long size = stream.Position - this.readOffset;
                if (this.readOffset < stream.Position)
                {
                    Array.Copy(stream.GetBuffer(), this.readOffset, stream.GetBuffer(), 0, stream.Position - this.readOffset);
                }
                //Reset Stream
                this.readOffset = 0;
                stream.Position = size;
                stream.SetLength(size);
            }
            return true;
        }
    }
}
