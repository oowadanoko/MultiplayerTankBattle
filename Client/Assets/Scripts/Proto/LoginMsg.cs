using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MsgRegister : MsgBase
{
    public string id = "";
    public string pw = "";
    public int result = 0;

    public MsgRegister()
    {
        protoName = "MsgRegister";
    }
}

public class MsgLogin : MsgBase
{
    public string id = "";
    public string pw = "";
    public int result = 0;

    public MsgLogin()
    {
        protoName = "MsgLogin";
    }
}

public class MsgKick : MsgBase
{
    public int reason = 0;
    public MsgKick()
    {
        protoName = "MsgKick";
    }
}