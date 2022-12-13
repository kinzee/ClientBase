using System;

namespace GFramework
{
    public abstract class GFrameworkEventArgs : EventArgs, IRef
    {
        public GFrameworkEventArgs()
        {
        }

        public abstract void Clear();
    }
}