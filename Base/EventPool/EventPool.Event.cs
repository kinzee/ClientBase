namespace GF
{
    public sealed partial class EventPool<T> where T : BaseEventArgs
    {
        private sealed class Event : IRef
        {
            private object _sender;
            private T _eventArgs;

            public Event()
            {
                _sender = null;
                _eventArgs = null;
            }

            public object Sender
            {
                get { return _sender; }
            }

            public T EventArgs
            {
                get { return _eventArgs; }
            }

            public static Event Create(object sender, T e)
            {
                Event eventNode = RefPool.Acquire<Event>();
                eventNode._sender = sender;
                eventNode._eventArgs = e;
                return eventNode;
            }

            public void Clear()
            {
                _sender = null;
                _eventArgs = null;
            }
        }
    }
}