using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ByteArray
{
    const int DEFAULT_SIZE = 1024;
    int initSize = 0;
    public byte[] bytes;
    public int readIdx = 0;//读索引
    public int writeIdx = 0;//写索引
    private int capacity = 0;

    /// <summary>
    /// 获取剩余容量
    /// </summary>
    public int remain
    {
        get
        {
            return capacity - writeIdx;
        }
    }

    /// <summary>
    /// 获取有效的消息长度
    /// </summary>
    public int length
    {
        get
        {
            return writeIdx - readIdx;
        }
    }

    public ByteArray(int size = DEFAULT_SIZE)
    {
        bytes = new byte[size];
        capacity = size;
        initSize = size;
        readIdx = 0;
        writeIdx = 0;
    }

    public ByteArray(byte[] defaultBytes)
    {
        bytes = defaultBytes;
        capacity = defaultBytes.Length;
        initSize = defaultBytes.Length;
        readIdx = 0;
        writeIdx = defaultBytes.Length;
    }

    public override string ToString()
    {
        return BitConverter.ToString(bytes, readIdx, length);
    }

    public string Debug()
    {
        return string.Format("readIdx({0}) writeIdx({1}) bytes({2})",
            readIdx, writeIdx, BitConverter.ToString(bytes, 0, bytes.Length));
    }

    public void ReSize(int size)
    {
        if (size < length)
        {
            return;
        }
        if (size < initSize)
        {
            return;
        }
        int n = 1;
        //扩容为2的倍数并比要求的容量大
        while (n < size)
        {
            n *= 2;
        }
        capacity = n;
        byte[] newBytes = new byte[capacity];
        Array.Copy(bytes, readIdx, newBytes, 0, writeIdx - readIdx);
        bytes = newBytes;
        writeIdx = length;
        readIdx = 0;
    }

    /// <summary>
    /// 若当前消息长度不足8个字节，将所有数据移动到数组开头
    /// </summary>
    public void CheckAndMoveBytes()
    {
        if (length < 8)
        {
            MoveBytes();
        }
    }

    /// <summary>
    /// 将所有数据移动到数组开头
    /// </summary>
    public void MoveBytes()
    {
        if (length > 0)
        {
            Array.Copy(bytes, readIdx, bytes, 0, length);
        }
        writeIdx = length;
        readIdx = 0;
    }

    /// <summary>
    /// 从字节数组写数据到该对象
    /// </summary>
    /// <param name="bs">源字节数组</param>
    /// <param name="offset">从源数组开始写的偏移量</param>
    /// <param name="count">要写入的字节数</param>
    /// <returns>实际写入的字节数</returns>
    public int Write(byte[] bs, int offset, int count)
    {
        if (remain < count)
        {
            ReSize(length + count);
        }
        Array.Copy(bs, offset, bytes, writeIdx, count);
        writeIdx += count;
        return count;
    }

    /// <summary>
    /// 从该对象读取数据到字节数组
    /// </summary>
    /// <param name="bs">目标字节数组</param>
    /// <param name="offset">要写入到目标数组位置的偏移量</param>
    /// <param name="count">要读取的字节数</param>
    /// <returns>实际读取的字节数</returns>
    public int Read(byte[] bs, int offset, int count)
    {
        count = Math.Min(count, length);
        Array.Copy(bytes, readIdx, bs, offset, count);
        readIdx += count;
        CheckAndMoveBytes();
        return count;
    }

    /// <summary>
    /// 从该对象消息中读取一个16位整数，以小端模式读取
    /// </summary>
    /// <returns>16位整数</returns>
    public Int16 ReadInt16()
    {
        if (length < 2)
        {
            return 0;
        }
        Int16 ret = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
        readIdx += 2;
        CheckAndMoveBytes();
        return ret;
    }

    /// <summary>
    /// 从该对象消息中读取一个32位整数，以小端模式读取
    /// </summary>
    /// <returns>32位整数</returns>
    public Int32 ReadInt32()
    {
        if (length < 4)
        {
            return 0;
        }
        Int32 ret = (Int32)((bytes[readIdx + 3] << 24) | (bytes[readIdx + 2] << 16) | (bytes[readIdx + 1] << 8) | bytes[readIdx]);
        readIdx += 4;
        CheckAndMoveBytes();
        return ret;
    }
}