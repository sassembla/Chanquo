using System.Collections;
using System.Collections.Generic;
using Chanquo.v2;
using UnityEngine;

public struct Data { }
public class Sample : MonoBehaviour
{
    private Chan<Data> ch = Chan<Data>.Make();

    // Start is called before the first frame update
    void Start()
    {
        ch.Receive(
            (data, ok) =>
            {
                if (!ok)
                {
                    Debug.Log("close!");
                    return;
                }
                Debug.Log("data:" + data);
            }
        );
    }

    // Update is called once per frame
    void Update()
    {
        ch.Send(new Data());
    }

    void OnApplicationQuit()
    {
        Debug.Log("quit!");
        Channels.Close<Data>();
    }
}
