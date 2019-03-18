using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ChanquoCore
{
    public class Chanquo
    {
        private static Chanquo _chanq;

        private Hashtable channelTable = new Hashtable();
        private GameObject go = new GameObject("ChanquoThreadRunner");
        private ChanquoThreadRunner runner = null;
        private Thread unityThread;
        private object channelWriteLock = new object();

        private ChanquoChannel AddChannel<T>() where T : ChanquoBase, new()
        {
            var key = typeof(T);
            if (channelTable.ContainsKey(key))
            {
                return (ChanquoChannel)channelTable[key];
            }

            // generate new channel.
            var chan = new ChanquoChannel(
                actIds =>
                {
                    lock (channelWriteLock)
                    {
                        channelTable.Remove(typeof(T));
                    }

                    runner.Dispose(actIds);
                }
            );
            channelTable[typeof(T)] = chan;
            return chan;
        }

        private ChanquoAction<T> AddReceiver<T>(Action<T> act, ThreadMode mode = ThreadMode.Default) where T : ChanquoBase, new()
        {
            // ここで関連付けを行う。
            ChanquoChannel chan;

            var key = typeof(T);
            if (channelTable.ContainsKey(key))
            {
                chan = (ChanquoChannel)channelTable[key];
            }
            else
            {
                chan = new ChanquoChannel(
                    actIds =>
                    {
                        lock (channelWriteLock)
                        {
                            channelTable.Remove(typeof(T));
                        }

                        runner.Dispose(actIds);
                    }
                );

                lock (channelWriteLock)
                {
                    channelTable[typeof(T)] = chan;
                }
            }

            var chanquoAct = new ChanquoAction<T>(act);
            var pullActAndId = chan.AddSelectAction<T>(chanquoAct);
            chanquoAct.SetOnDispose(() =>
            {
                chan.Remove(pullActAndId.id);
                runner.Dispose(new List<string> { pullActAndId.id });
            });

            if (Thread.CurrentThread == unityThread)
            {
                switch (mode)
                {
                    case ThreadMode.Default:
                        runner.Add(pullActAndId.id, pullActAndId.pullAct, ThreadMode.OnUpdate);
                        break;
                    default:
                        runner.Add(pullActAndId.id, pullActAndId.pullAct, mode);
                        break;
                }
            }
            else
            {
                // recever is not waiting on UnityThread.
                chan.AddNonUnityThreadSelectAct(chanquoAct);

                Task.Delay(TimeSpan.FromTicks(1)).ContinueWith(o =>
                {
                    T s;
                    while ((s = chan.Dequeue<T>()) != null)
                    {
                        chanquoAct.act?.Invoke(s);
                    }
                });
            }

            return chanquoAct;
        }

        // GameObjectの制約の関係で、初期化はMainThreadからしかできなそう。
        static Chanquo()
        {
            _chanq = new Chanquo();
            _chanq.runner = _chanq.go.AddComponent<ChanquoThreadRunner>();
            GameObject.DontDestroyOnLoad(_chanq.go);
            _chanq.unityThread = Thread.CurrentThread;
        }


        public static ChanquoChannel MakeChannel<T>() where T : ChanquoBase, new()
        {
            return _chanq.AddChannel<T>();
        }


        public static T Receive<T>() where T : ChanquoBase, new()
        {
            if (_chanq.channelTable.ContainsKey(typeof(T)))
            {
                var chan = (ChanquoChannel)_chanq.channelTable[typeof(T)];
                return chan?.Dequeue<T>();
            }

            var newChan = _chanq.AddChannel<T>();
            return newChan.Dequeue<T>();// return null val.
        }

        public static ChanquoAction<T> Select<T>(Action<T> act, ThreadMode mode = ThreadMode.Default) where T : ChanquoBase, new()
        {
            return _chanq.AddReceiver(act, mode);
        }

        public static ChanquoAction<T, U> Select<T, U>(Action<T> act1, Action<U> act2, ThreadMode mode = ThreadMode.Default)
        where T : ChanquoBase, new()
        where U : ChanquoBase, new()
        {
            var cAct1 = _chanq.AddReceiver(act1, mode);
            var cAct2 = _chanq.AddReceiver(act2, mode);
            return new ChanquoAction<T, U>(cAct1, cAct2);
        }

        public static ChanquoAction<T, U, V> Select<T, U, V>(Action<T> act1, Action<U> act2, Action<V> act3, ThreadMode mode = ThreadMode.Default)
        where T : ChanquoBase, new()
        where U : ChanquoBase, new()
        where V : ChanquoBase, new()
        {
            var cAct1 = _chanq.AddReceiver(act1, mode);
            var cAct2 = _chanq.AddReceiver(act2, mode);
            var cAct3 = _chanq.AddReceiver(act3, mode);
            return new ChanquoAction<T, U, V>(cAct1, cAct2, cAct3);
        }

        public static ChanquoAction<T, U, V, W> Select<T, U, V, W>(Action<T> act1, Action<U> act2, Action<V> act3, Action<W> act4, ThreadMode mode = ThreadMode.Default)
        where T : ChanquoBase, new()
        where U : ChanquoBase, new()
        where V : ChanquoBase, new()
        where W : ChanquoBase, new()
        {
            var cAct1 = _chanq.AddReceiver(act1, mode);
            var cAct2 = _chanq.AddReceiver(act2, mode);
            var cAct3 = _chanq.AddReceiver(act3, mode);
            var cAct4 = _chanq.AddReceiver(act4, mode);
            return new ChanquoAction<T, U, V, W>(cAct1, cAct2, cAct3, cAct4);
        }
    }
}