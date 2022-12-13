using System;
using System.Collections.Generic;

namespace GFramework
{
    /// <summary>
    /// 事件池模式。
    /// </summary>
    [Flags]
    public enum EventPoolMode : byte
    {
        /// <summary>
        /// 默认事件池模式，即必须存在有且只有一个事件处理函数。
        /// </summary>
        Default = 0,

        /// <summary>
        /// 允许不存在事件处理函数。
        /// </summary>
        AllowNoHandler = 1,

        /// <summary>
        /// 允许存在多个事件处理函数。
        /// </summary>
        AllowMultiHandler = 2,

        /// <summary>
        /// 允许存在重复的事件处理函数。
        /// </summary>
        AllowDuplicateHandler = 4
    }

    public sealed partial class EventPool<T> where T : BaseEventArgs
    {
        private readonly MultiDictionary<int, EventHandler<T>> _eventHandlers;
        private readonly Queue<Event> _events; // 事件执行队列
        private readonly Dictionary<object, LinkedListNode<EventHandler<T>>> _cachedNodes;
        private readonly Dictionary<object, LinkedListNode<EventHandler<T>>> _tmpNodes;
        private readonly EventPoolMode _eventPoolMode;
        private EventHandler<T> _defaultHandler;

        /// <summary>
        /// 初始化事件池新实例
        /// </summary>
        /// <param name="eventPoolMode">事件池模式</param>
        public EventPool(EventPoolMode eventPoolMode)
        {
            _eventPoolMode = eventPoolMode;
            _eventHandlers = new MultiDictionary<int, EventHandler<T>>();
            _cachedNodes = new Dictionary<object, LinkedListNode<EventHandler<T>>>();
            _tmpNodes = new Dictionary<object, LinkedListNode<EventHandler<T>>>();
            _events = new Queue<Event>();
            _defaultHandler = null;
        }

        /// <summary>
        /// 获取事件处理函数的数量
        /// </summary>
        public int EventHandlerCount => _eventHandlers.Count;

        /// <summary>
        /// 获取要处理的事件数量
        /// </summary>
        public int EventCount => _events.Count;
        
        /// <summary>
        /// 获取事件处理函数的数量
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Count(int id)
        {
            if (_eventHandlers.TryGetValue(id, out var range))
            {
                return range.Count;
            }

            return 0;
        }

        /// <summary>
        /// 检查是否存在事件处理函数
        /// </summary>
        /// <param name="id">事件id</param>
        /// <param name="handler">事件处理函数</param>
        /// <returns>是否存在</returns>
        /// <exception cref="GFrameworkException"></exception>
        public bool Contains(int id, EventHandler<T> handler)
        {
            if (handler == null)
            {
                throw new GFrameworkException("Event handler is invalid");
            }

            return _eventHandlers.Contains(id, handler);
        }
        
        /// <summary>
        /// 事件池轮询
        /// </summary>
        public void Update()
        {
            lock (_events)
            {
                while (_events.Count > 0)
                {
                    Event eventNode = _events.Dequeue();
                    HandleEvent(eventNode.Sender, eventNode.EventArgs);
                    RefPool.Release(eventNode);
                }
            }
        }

        /// <summary>
        /// 清理事件
        /// </summary>
        public void Clear()
        {
            lock (_events)
            {
                _events.Clear();
            }
        }

        /// <summary>
        /// 关闭并清理事件池
        /// </summary>
        public void Shutdown()
        {
            Clear();
            _eventHandlers.Clear();
            _defaultHandler = null;
            _cachedNodes.Clear();
            _tmpNodes.Clear();
        }

        /// <summary>
        /// 订阅事件处理函数
        /// </summary>
        /// <param name="id">事件类型id</param>
        /// <param name="handler">要订阅的事件处理函数</param>
        /// <exception cref="GFrameworkException"></exception>
        public void Subscribe(int id, EventHandler<T> handler)
        {
            if (handler == null)
            {
                throw new GFrameworkException("Event handler is invalid");
            }

            if (!_eventHandlers.Contains(id))
            {
                _eventHandlers.Add(id, handler);
            }
            else if ((_eventPoolMode & EventPoolMode.AllowDuplicateHandler) != EventPoolMode.AllowMultiHandler)
            {
                throw new GFrameworkException(string.Format("Event'{0} not allow multi handler", id));
            }
            else if ((_eventPoolMode & EventPoolMode.AllowDuplicateHandler) != EventPoolMode.AllowDuplicateHandler && Contains(id, handler))
            {
                throw new GFrameworkException(string.Format("Event'{0} not allow duplicate handler", id));
            }
            else
            {
                _eventHandlers.Add(id, handler);
            }
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="id">事件id</param>
        /// <param name="handler">事件处理函数</param>
        /// <exception cref="GameFrameworkException"></exception>
        public void Unsubscribe(int id, EventHandler<T> handler)
        {
            if (handler == null)
            {
                throw new GFrameworkException("Event handler is invalid.");
            }

            if (_cachedNodes.Count > 0)
            {
                foreach (var cachedNode in _cachedNodes)
                {
                    if (cachedNode.Value != null && cachedNode.Value.Value == handler)
                    {
                        _tmpNodes.Add(cachedNode.Key, cachedNode.Value.Next);
                    }
                }

                if (_tmpNodes.Count > 0)
                {
                    foreach (var cachedNode in _tmpNodes)
                    {
                        _cachedNodes[cachedNode.Key] = cachedNode.Value;
                    }
                    
                    _tmpNodes.Clear();
                }
            }

            if (!_eventHandlers.Remove(id, handler))
            {
                throw new GFrameworkException(string.Format("Event '{0}' not exists specified handler", id));
            }
        }

        /// <summary>
        /// 设置默认事件处理函数。
        /// </summary>
        /// <param name="handler">要设置的默认事件处理函数。</param>
        public void SetDefaultHandler(EventHandler<T> handler)
        {
            _defaultHandler = handler;
        }

        /// <summary>
        /// 抛出事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        /// <exception cref="GFrameworkException"></exception>
        public void Fire(object sender, T e)
        {
            if (e==null)
            {
                throw new GFrameworkException("Event is invalid.");
            }
            
            Event eventNode = Event.Create(sender, e);
            lock (_events)
            {
                _events.Enqueue(eventNode);
            }
        }
        
        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        public void FireNow(object sender, T e)
        {
            if (e == null)
            {
                throw new GFrameworkException("Event is invalid.");
            }

            HandleEvent(sender, e);
        }

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleEvent(object sender, T e)
        {
            bool noHandlerException = false;
            if (_eventHandlers.TryGetValue(e.Id, out var range))
            {
                var current = range.First;
                while (current != null && current != range.Terminal)
                {
                    _cachedNodes[e] = current.Next != range.Terminal ? current.Next : null;
                    current.Value(sender, e);
                    current = _cachedNodes[e];
                }

                _cachedNodes.Remove(e);
            }
            else if (_defaultHandler != null)
            {
                _defaultHandler(sender, e);
            }
            else if ((_eventPoolMode & EventPoolMode.AllowNoHandler) == 0)
            {
                noHandlerException = true;
            }
            RefPool.Release(e);
            if (noHandlerException)
            {
                throw new GFrameworkException(string.Format("Event '{0}' not allow no handler.", e.Id));
            }
        }
    }
}