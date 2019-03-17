using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace ChanquoCore
{
    public class ChanquoThreadRunner : MonoBehaviour
    {
        private ConcurrentBag<Action> update = new ConcurrentBag<Action>();

        public void Add(Action act, ThreadMode mode)
        {
            switch (mode)
            {
                case ThreadMode.OnUpdate:
                    update.Add(act);
                    break;
                default:
                    throw new Exception("unsupported mode:" + mode);
            }
        }

        public void Update()
        {
            foreach (var upd in update)
            {
                if (upd != null)
                {
                    upd();
                }
                else
                {
                    Debug.Log("こいつはnull");
                    Action a;
                    update.TryTake(out a);
                }
            }
        }
    }
}