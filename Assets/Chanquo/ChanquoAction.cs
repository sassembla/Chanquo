using System;

namespace ChanquoCore
{
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