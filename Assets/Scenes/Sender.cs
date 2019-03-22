using System.Collections;
using System.Collections.Generic;
using ChanquoCore;
using UnityEngine;

public class Sender : MonoBehaviour
{
    public class Something : IChanquoBase
    {
        public string message;
    }

    // Start is called before the first frame update
    void Start()
    {
        var c = Chanquo.MakeChannel<Something>();
        c.Send(new Something { message = "dummy" });
        Debug.Log("send frame:" + Time.frameCount);
    }

}
