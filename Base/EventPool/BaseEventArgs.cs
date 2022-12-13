namespace GFramework
{
    /// <summary>
    /// 事件基类
    /// </summary>
    public abstract class BaseEventArgs : GFrameworkEventArgs
    {
        public abstract int Id { get; }
    }
}