using System;

namespace GF
{
    public abstract class GFEventArgs : EventArgs, IRef
    {
        public GFEventArgs()
        {
        }

        public abstract void Clear();
    }
}