using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

public class MsgBase
{
    /// <summary>
    /// 协议名，后续需要根据协议名通过反射进行操作
    /// </summary>
    public string protoName { get; set; } = "";

    /// <summary>
    /// 将消息编码成json字节数组
    /// </summary>
    /// <param name="msgBase">消息</param>
    /// <returns>json字节数组</returns>
    public static byte[] Encode(MsgBase msgBase)
    {
        string s = JsonSerializer.Serialize(msgBase, msgBase.GetType());
        //Console.WriteLine("发送" + msgBase.protoName + ": " + s);
        return System.Text.Encoding.UTF8.GetBytes(s);
    }

    /// <summary>
    /// 将字节数组反序列化为一个消息对象
    /// </summary>
    /// <param name="protoName">协议名，会将字节数组反序列为协议名指定的类型</param>
    /// <param name="bytes">字节数组</param>
    /// <param name="offset">字节数组的偏移量</param>
    /// <param name="count">字节个数</param>
    /// <returns>反序列化得到的消息对象</returns>
    public static MsgBase Decode(string protoName, byte[] bytes, int offset, int count)
    {
        string s = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
        //Console.WriteLine("接收" + protoName + ": " + s);
        MsgBase msgBase = JsonSerializer.Deserialize(s, Type.GetType(protoName)) as MsgBase;
        return msgBase;
    }

    /// <summary>
    /// 将消息名编码为字节数组，会在字节数组开头写入消息名长度
    /// </summary>
    /// <param name="msgBase">消息</param>
    /// <returns>字节数组</returns>
    public static byte[] EncodeName(MsgBase msgBase)
    {
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.protoName);
        Int16 len = (Int16)nameBytes.Length;//消息名长度
        byte[] bytes = new byte[2 + len];//开头加上消息名的长度
        //以小端模式编码整数
        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);
        Array.Copy(nameBytes, 0, bytes, 2, len);
        return bytes;
    }

    /// <summary>
    /// 从字节数组中获取消息名
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <param name="offset">读取字节数组的偏移量</param>
    /// <param name="count">实际读取的字节个数</param>
    /// <returns>消息名</returns>
    public static string DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;
        if (offset + 2 > bytes.Length)//内容太少，连一个表示消息名长度的16位整数都没有
        {
            return "";
        }
        Int16 len = (Int16)((bytes[offset + 1] << 8) | bytes[offset]);//消息长度
        if (len <= 0)
        {
            return "";
        }
        if (offset + 2 + len > bytes.Length)//消息不完整，还没有一个完整的消息名
        {
            return "";
        }
        count = 2 + len;
        string name = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);
        return name;
    }
}
