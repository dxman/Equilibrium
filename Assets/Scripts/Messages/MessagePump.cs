using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Equilibrium.Messages
{
    public class MessagePump : IMessagePublisher
    {
        public static Action<object, object> HandlerResultProcessing = (target, result) => { };
        private readonly List<IMessagePublisher> _children = new List<IMessagePublisher>();

        private readonly List<WeakMessageHandler> _handlers = new List<WeakMessageHandler>();

        public void Publish(object message)
        {
            Publish(message, action => action());
        }

        public bool HandlerExistsFor(Type messageType)
        {
            lock (_handlers)
            {
                return _handlers.Any(handler => handler.Handles(messageType) && !handler.IsDead);
            }
        }

        public void Subscribe(object subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));

            lock (_handlers)
            {
                if (!_handlers.Any(x => x.Matches(subscriber)))
                    _handlers.Add(new WeakMessageHandler(subscriber));
            }
        }

        public void Unsubscribe(object subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));

            lock (_handlers)
            {
                _handlers.RemoveAll(x => x.Matches(subscriber));
            }
        }

        public void AddChild(IMessagePublisher child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            lock (_children)
            {
                if (_children.All(x => x != child))
                    _children.Add(child);
            }
        }

        public void RemoveChild(IMessagePublisher child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            lock (_children)
            {
                _children.RemoveAll(x => x == child);
            }
        }

        private void Publish(object message, Action<Action> marshal)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (marshal == null)
                throw new ArgumentNullException(nameof(marshal));

            WeakMessageHandler[] toNotify;
            lock (_handlers)
            {
                toNotify = _handlers.ToArray();
            }

            marshal(() =>
            {
                var messageType = message.GetType();

                var dead = toNotify
                    .Where(handler => !handler.Handle(messageType, message))
                    .ToList();

                if (dead.Any())
                    lock (_handlers)
                    {
                        foreach (var handler in dead)
                            _handlers.Remove(handler);
                    }
            });

            lock (_children)
            {
                _children.ForEach(x => x.Publish(message));
            }
        }

        private class WeakMessageHandler
        {
            private readonly Dictionary<Type, MethodInfo> _supportedHandlers;
            private readonly WeakReference _weakReference;

            public WeakMessageHandler(object handler)
            {
                _weakReference = new WeakReference(handler);
                _supportedHandlers = new Dictionary<Type, MethodInfo>();

                var interfaces =
                    handler.GetType().GetInterfaces().Where(x => typeof(IMessageSubscriber).IsAssignableFrom(x)
                                                                 && x.IsGenericType);

                foreach (var i in interfaces)
                {
                    var type = i.GetGenericArguments()[0];
                    var method = i.GetMethod("Handle");
                    _supportedHandlers[type] = method;
                }
            }

            public bool IsDead => _weakReference.Target == null;

            public bool Matches(object instance)
            {
                return _weakReference.Target == instance;
            }

            public bool Handle(Type messageType, object message)
            {
                var target = _weakReference.Target;
                if (target == null)
                    return false;

                foreach (var pair in _supportedHandlers)
                    if (pair.Key.IsAssignableFrom(messageType))
                    {
                        var result = pair.Value.Invoke(target, new[] {message});
                        if (result != null)
                            HandlerResultProcessing(target, result);
                    }

                return true;
            }

            public bool Handles(Type messageType)
            {
                return _supportedHandlers.Any(pair => pair.Key.IsAssignableFrom(messageType));
            }
        }
    }

    public interface IMessagePublisher
    {
        void Publish(object message);
    }

    public interface IMessageSubscriber
    {
    }

    public interface IMessageSubscriber<in TMessage> : IMessageSubscriber
    {
        void Handle(TMessage message);
    }
}