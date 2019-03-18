using System;

namespace ChanquoCore
{
    public class ChanquoAction<T, U> : IDisposable
        where T : ChanquoBase, new()
        where U : ChanquoBase, new()
    {
        private ChanquoAction<T> cAct1;
        private ChanquoAction<U> cAct2;

        public ChanquoAction(ChanquoAction<T> cAct1, ChanquoAction<U> cAct2)
        {
            this.cAct1 = cAct1;
            this.cAct2 = cAct2;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cAct1.Dispose();
                    cAct2.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ChanquoAction() {
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

    public class ChanquoAction<T, U, V> : IDisposable
        where T : ChanquoBase, new()
        where U : ChanquoBase, new()
        where V : ChanquoBase, new()
    {
        private ChanquoAction<T> cAct1;
        private ChanquoAction<U> cAct2;
        private ChanquoAction<V> cAct3;

        public ChanquoAction(ChanquoAction<T> cAct1, ChanquoAction<U> cAct2, ChanquoAction<V> cAct3)
        {
            this.cAct1 = cAct1;
            this.cAct2 = cAct2;
            this.cAct3 = cAct3;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cAct1.Dispose();
                    cAct2.Dispose();
                    cAct3.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ChanquoAction() {
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

    public class ChanquoAction<T, U, V, W> : IDisposable
        where T : ChanquoBase, new()
        where U : ChanquoBase, new()
        where V : ChanquoBase, new()
        where W : ChanquoBase, new()
    {
        private ChanquoAction<T> cAct1;
        private ChanquoAction<U> cAct2;
        private ChanquoAction<V> cAct3;
        private ChanquoAction<W> cAct4;

        public ChanquoAction(ChanquoAction<T> cAct1, ChanquoAction<U> cAct2, ChanquoAction<V> cAct3, ChanquoAction<W> cAct4)
        {
            this.cAct1 = cAct1;
            this.cAct2 = cAct2;
            this.cAct3 = cAct3;
            this.cAct4 = cAct4;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cAct1.Dispose();
                    cAct2.Dispose();
                    cAct3.Dispose();
                    cAct4.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ChanquoAction() {
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

    public class ChanquoAction<T> : IDisposable where T : ChanquoBase, new()
    {
        // このインスタンスのactは、存在し続けるだけでChanquo本体から実行される。
        // disposeすると受け取る権利を失う。
        public readonly Action<T> act;
        private Action onDispose;
        public ChanquoAction(Action<T> act)
        {
            this.act = act;
        }

        public void SetOnDispose(Action onDispose)
        {
            this.onDispose = onDispose;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    onDispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ChanquoAction() {
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