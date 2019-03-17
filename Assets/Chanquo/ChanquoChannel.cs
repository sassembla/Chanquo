using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace ChanquoCore
{
    public class ChanquoChannel : IDisposable
    {
        private ConcurrentQueue<IChanquoBase> queue = new ConcurrentQueue<IChanquoBase>();
        private Hashtable selectActTable = new Hashtable();
        private Hashtable nonUnityThreadSelectActTable = new Hashtable();
        private object actTableLock = new object();

        public void Send<T>(T data) where T : class, IChanquoBase, new()
        {
            queue?.Enqueue(data);
            foreach (var id in nonUnityThreadSelectActTable)
            {
                ((Action)nonUnityThreadSelectActTable[id])?.Invoke();
            }
        }

        internal T Dequeue<T>() where T : class, IChanquoBase, new()
        {
            if (disposedValue || queue.Count == 0)
            {
                return null;
            }

            IChanquoBase result;
            queue.TryDequeue(out result);
            return (T)result;
        }

        public Action AddSelectAction<T>(ChanquoAction<T> selectAct) where T : class, IChanquoBase, new()
        {
            if (disposedValue)
            {
                return () => { };
            }

            var id = Guid.NewGuid().ToString();
            Action pullAct = () =>
            {
                var count = queue.Count;
                for (var i = 0; i < count; i++)
                {
                    selectAct.act(Dequeue<T>());
                }
            };

            lock (actTableLock)
            {
                selectActTable[id] = pullAct;
            }

            return pullAct;
        }

        public void AddNonUnityThreadSelectAct<T>(ChanquoAction<T> selectAct) where T : class, IChanquoBase, new()
        {
            var id = Guid.NewGuid().ToString();
            Action pullAct = () =>
            {
                var count = queue.Count;
                for (var i = 0; i < count; i++)
                {
                    selectAct.act(Dequeue<T>());
                }
            };

            lock (actTableLock)
            {
                nonUnityThreadSelectActTable[id] = pullAct;
            }
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // これが実行されたら、もうこのchannelは停止する。登録されている全てのreceiverを消す。
                    var pullActIds = selectActTable.Values;
                    lock (actTableLock)
                    {
                        foreach (var id in pullActIds)
                        {
                            selectActTable[id] = null;
                            nonUnityThreadSelectActTable[id] = null;
                        }
                    }

                    queue = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ChanquoChannel() {
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
}