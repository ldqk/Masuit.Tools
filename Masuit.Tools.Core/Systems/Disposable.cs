using System;

namespace Masuit.Tools.Core.Systems
{
    public abstract class Disposable : IDisposable
    {
        private bool isDisposed;

        ~Disposable()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            isDisposed = true;
            GC.SuppressFinalize(this);
        }

        public abstract void Dispose(bool disposing);
    }
}
