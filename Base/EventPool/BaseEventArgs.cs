namespace GF
{
    /// <summary>
    /// 事件基类
    /// </summary>
    public abstract class BaseEventArgs : GFEventArgs
    {
        public abstract int Id { get; }
    }
}