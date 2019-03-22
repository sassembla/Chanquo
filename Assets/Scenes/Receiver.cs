using ChanquoCore;
using UnityEngine;

public class Receiver : MonoBehaviour
{
    void Start()
    {
        var s = Chanquo.Select<Sender.Something>(
            (something, ok) =>
            {
                if (!ok)
                {
                    return;
                }
                Debug.Log("受け取った。 message:" + something.message + " frame:" + Time.frameCount);
            }
        );
    }
}