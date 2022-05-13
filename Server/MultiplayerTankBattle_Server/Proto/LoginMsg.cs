using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MsgRegister : MsgBase
{
    public string id { get; set; } = "";
    public string pw { get; set; } = "";
    public int result { get; set; } = 0;

    public MsgRegister()
    {
        protoName = "MsgRegister";
    }
}

public class MsgLogin : MsgBase
{
    public string id { get; set; } = "";
    public string pw { get; set; } = "";
    public int result { get; set; } = 0;

    public MsgLogin()
    {
        protoName = "MsgLogin";
    }
}

public class MsgKick : MsgBase
{
    public int reason { get; set; } = 0;
    public MsgKick()
    {
        protoName = "MsgKick";
    }
}