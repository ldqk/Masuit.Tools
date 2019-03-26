using System;

namespace Masuit.Tools.Systems
{
    /// <summary>
    /// Disposable
    /// </summary>
    public abstract class Disposable : IDisposable
    {
        private bool isDisposed;

        /// <summary>
        /// 终结器
        /// </summary>
        ~Disposable()
        {
            Dispose(false);
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="disposing"></param>
        public abstract void Dispose(bool disposing);
    }
}