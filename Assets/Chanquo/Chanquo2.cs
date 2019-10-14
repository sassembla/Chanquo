using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace Chanquo.v2
{
    public partial class Chan<P> : IDisposable where P : struct
    {
        private ConcurrentQueue<P> q = new ConcurrentQueue<P>();
        private Action<P, bool> receiveAct;

        public void Send(P data)
        {
            _Send(data, true);
        }

        private void _Send(P data, bool isOpen)
        {
            if (!chs.IsMainThread())
            {
                // メインスレッドではないスレッドからの送信は、受信側がどんなスレッドで受け取りたいかを加味してセットを行う。
                q.Enqueue(data);
                return;
            }

            if (receiveAct != null)
            {
                receiveAct(data, isOpen);
                return;
            }

            if (isOpen)
            {
                // 受信側が存在しない場合、データを詰める。
                q.Enqueue(data);
            }
        }

        public void Receive(Action<P, bool> act)
        {
            // セットまでに積まれているデータを受け取る
            P data;
            while (q.TryDequeue(out data))
            {
                act(data, true);
            }

            receiveAct = act;

            // メインスレッド外からの送信に対して、定期的な受信をセットする。
            chs.Add<P>(
                () =>
                {
                    if (q.Count == 0)
                    {
                        return;
                    }

                    P data2;
                    while (q.TryDequeue(out data2))
                    {
                        receiveAct(data2, true);
                    }
                }
            );
        }

        public void Close()
        {
            Dispose();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _Send(new P(), false);
                    receiveAct = (a, b) => { };
                    chs.Remove<P>();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Channel()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }

    public partial class Chan<P> : IDisposable where P : struct
    {
        private static Channels chs = new Channels();

        public static Chan<P> Make()
        {
            return chs.GetOrCreate<P>();
        }

        public static void Close<T>() where T : struct
        {
            Chan<T> ch;
            if (chs.IsExists<T>(out ch))
            {
                if (ch != null)
                {
                    ch.Dispose();
                }
                return;
            }
        }

        public static bool IsExists<T>() where T : struct
        {
            Chan<T> ch;
            return chs.IsExists<T>(out ch);
        }

        // 待つ、値を取得しない
        public static IEnumerator WaitFirst<T>() where T : struct
        {
            var done = false;
            var ch = chs.GetOrCreate<T>();
            ch.Receive(
                (a, ok) =>
                {
                    done = true;
                }
            );
            while (!done)
            {
                yield return null;
            }
        }

        // 待つ、値を取得する
        public static WaitResultInstruction<T> WaitFirstResult<T>() where T : struct
        {
            return new WaitResultInstruction<T>();
        }

        public static ForInstruction<T> For<T>(Action<T> onReceive) where T : struct
        {
            return new ForInstruction<T>(onReceive);
        }
    }

    public partial class Channels
    {
        private object lockObj = new object();
        private static Thread thread;
        private static ChRunner runner;
        private readonly Hashtable chs = new Hashtable();

        [RuntimeInitializeOnLoadMethod]
        static void Initialize()
        {
            // mainThreadを補足する。
            thread = Thread.CurrentThread;

            // pull action用のgameObjectを生成する。
            var go = new GameObject("ChanquoRunner");
            GameObject.DontDestroyOnLoad(go);
            runner = go.AddComponent<ChRunner>();
        }

        internal Chan<T> GetOrCreate<T>() where T : struct
        {
            lock (lockObj)
            {
                var type = typeof(T);
                if (chs.ContainsKey(type))
                {
                    return (Chan<T>)chs[type];
                }

                chs[type] = new Chan<T>();
                return (Chan<T>)chs[type];
            }
        }

        internal void Add<T>(Action pullAct) where T : struct
        {
            runner.Add<T>(pullAct);
        }

        internal void Remove<T>() where T : struct
        {
            lock (lockObj)
            {
                var type = typeof(T);
                if (chs.ContainsKey(type))
                {
                    chs.Remove(type);
                    runner.Remove<T>();
                    return;
                }
            }
        }

        internal bool IsExists<T>(out Chan<T> ch) where T : struct
        {
            lock (lockObj)
            {
                var type = typeof(T);
                if (chs.ContainsKey(type))
                {
                    ch = (Chan<T>)chs[type];
                    return true;
                }

                ch = null;
                return false;
            }
        }

        internal bool IsMainThread()
        {
            return thread == Thread.CurrentThread;
        }
    }


    // static handlers
    public partial class Channels
    {
        public static void Close<T>() where T : struct
        {
            Chan<T>.Close<T>();
        }

        public static bool IsExists<T>() where T : struct
        {
            return Chan<T>.IsExists<T>();
        }

        public static IEnumerator WaitFirst<T>() where T : struct
        {
            return Chan<T>.WaitFirst<T>();
        }

        public static WaitResultInstruction<T> WaitFirstResult<T>() where T : struct
        {
            return Chan<T>.WaitFirstResult<T>();
        }

        public static ForInstruction<T> For<T>(Action<T> onReceive) where T : struct
        {
            return Chan<T>.For<T>(onReceive);
        }
    }


    public class WaitResultInstruction<T> : CustomYieldInstruction where T : struct
    {
        private T _result = new T();
        private bool _keepWaiting = true;
        public override bool keepWaiting => _keepWaiting;

        public bool IsDone => _keepWaiting;

        public T Result => _result;

        public WaitResultInstruction()
        {
            var ch = Chan<T>.Make();
            ch.Receive(
                (result, ok) =>
                {
                    if (ok)
                    {
                        _result = result;
                    }
                    _keepWaiting = false;
                }
            );
        }
    }

    public class ForInstruction<T> : CustomYieldInstruction where T : struct
    {
        private bool _keepWaiting = true;
        public override bool keepWaiting => _keepWaiting;

        public bool IsDone => !_keepWaiting;

        public ForInstruction(Action<T> onReceive)
        {
            var ch = Chan<T>.Make();
            ch.Receive(
                (t, ok) =>
                {
                    if (!ok)
                    {
                        _keepWaiting = false;
                        return;
                    }
                    onReceive(t);
                }
            );
        }
    }


}