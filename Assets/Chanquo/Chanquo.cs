using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ChanquoCore
{
    public class Chanquo
    {
        private static Chanquo _chanq;

        private ConcurrentDictionary<Type, ChanquoChannel> channelDict = new ConcurrentDictionary<Type, ChanquoChannel>();
        private GameObject go = new GameObject("ChanquoThreadRunner");
        private ChanquoThreadRunner runner = null;
        private Thread unityThread;

        private ChanquoChannel AddChannel<T>() where T : IChanquoBase, new()
        {
            var key = typeof(T);
            if (channelDict.ContainsKey(key))
            {
                ChanquoChannel q;
                channelDict.TryGetValue(key, out q);
                return q;
            }

            // generate new channel.
            var chan = new ChanquoChannel();
            channelDict[typeof(T)] = chan;
            return chan;
        }

        private ChanquoAction<T> AddReceiver<T>(Action<T> act, ThreadMode mode = ThreadMode.Default) where T : class, IChanquoBase, new()
        {
            var chanquoAct = new ChanquoAction<T>(act);

            // ここで関連付けを行う。
            ChanquoChannel chan;
            var key = typeof(T);
            if (channelDict.ContainsKey(key))
            {
                channelDict.TryGetValue(key, out chan);
            }
            else
            {
                chan = new ChanquoChannel();
                channelDict[typeof(T)] = chan;
            }

            // chan自体が死んだら死ぬために、Actを渡したい。
            var pullAct = chan.AddSelectAction<T>(chanquoAct);
            if (Thread.CurrentThread == unityThread)
            {
                switch (mode)
                {
                    case ThreadMode.Default:
                        runner.Add(pullAct, ThreadMode.OnUpdate);
                        break;
                    default:
                        runner.Add(pullAct, mode);
                        break;
                }
            }
            else
            {
                // recever is not waiting on UnityThread.
                chan.AddNonUnityThreadSelectAct(chanquoAct);
            }

            // if current chan has some data, fire receiver action.
            T s;
            while ((s = chan.Dequeue<T>()) != null)
            {
                chanquoAct.act?.Invoke(s);
            }

            return chanquoAct;
        }

        // GameObjectの制約の関係で、初期化はMainThreadからしかできなそう。
        static Chanquo()
        {
            _chanq = new Chanquo();
            _chanq.runner = _chanq.go.AddComponent<ChanquoThreadRunner>();
            _chanq.unityThread = Thread.CurrentThread;
        }


        public static ChanquoChannel New<T>() where T : class, IChanquoBase, new()
        {
            return _chanq.AddChannel<T>();
        }


        public static T Receive<T>() where T : class, IChanquoBase, new()
        {
            if (_chanq.channelDict.ContainsKey(typeof(T)))
            {
                ChanquoChannel chan;
                _chanq.channelDict.TryGetValue(typeof(T), out chan);
                return chan?.Dequeue<T>();
            }

            var newChan = _chanq.AddChannel<T>();
            return newChan.Dequeue<T>();// return null val.
        }

        public static ChanquoAction<T> Select<T>(Action<T> act, ThreadMode mode = ThreadMode.Default) where T : class, IChanquoBase, new()
        {
            return _chanq.AddReceiver(act, mode);
        }
    }
}