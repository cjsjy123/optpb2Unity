using UnityEngine;
using System.IO;
using Google.Protobuf;
using Protobuf;
using System.Collections.Generic;

public class Test : MonoBehaviour
{
    public bool debug = false;


    void Update()
    {
        TheMsg msg = new TheMsg();
        msg.Name = "am the name";
        msg.Num = 32;

        if(debug)
            Debug.Log(string.Format("The Msg is ( Name:{0},Num:{1} )", msg.Name, msg.Num));
        TheMsg msg2 = null;
        using (var ms = new MemoryStream(msg.CalculateSize()))
        {
            msg.WriteTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            msg2 = TheMsg.Parser.ParseFrom(ms);
        }

        if (debug)
            Debug.Log(string.Format("The Msg2 is ( Name:{0},Num:{1} )", msg2.Name, msg2.Num));


        //新建一个Person对象，并赋值
        Person p = new Person();
        p.Name = "IongX";
        p.Age = Time.frameCount;
        p.NameList.Capacity = 4;
        p.NameList.Add("熊");
        p.NameList.Add("棒");
        p.NameList.Add("棒");
        //将对象转换成字节数组
        byte[] databytes = p.ToByteArray();

        //将字节数据的数据还原到对象中
        IMessage IMperson = new Person();
        Person p1 = new Person();
        p1 = (Person)IMperson.Descriptor.Parser.ParseFrom(databytes);

        //输出测试
        if (debug)
        {
            Debug.Log(p1.Name);
            Debug.Log(p1.Age);
            for (int i = 0; i < p1.NameList.Count; i++)
            {
                Debug.Log(p1.NameList[i]);
            }
            Debug.LogError("----------------");
        }
    }

    void OnApplicationQuit()
    {
        MessageExtensions.PoolDispose();
    }

}
