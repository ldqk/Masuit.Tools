using System;

namespace Masuit.Tools.Systems
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
            if (isDisposed)
            {
                return;
            }
            Dispose(true);
            isDisposed = true;
            GC.SuppressFinalize(this);
        }

        public abstract void Dispose(bool disposing);
    }
}
