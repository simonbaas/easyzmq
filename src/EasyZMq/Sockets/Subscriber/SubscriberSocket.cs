using System;
using EasyZMq.Infrastructure;
using EasyZMq.Logging;
using EasyZMq.Serialization;
using NetMQ;

namespace EasyZMq.Sockets.Subscriber
{
    public class SubscriberSocket : AbstractReceiverSocket, ISubscriberSocket
    {
        private IMessageDispatcher _dispatcher;

        public SubscriberSocket(ISerializer serializer, IAddressBinder addressBinder, ILoggerFactory loggerFactory, NetMQContext context, NetMQSocket socket)
            : base(serializer, addressBinder, loggerFactory, context, socket)
        {
            _dispatcher = new MessageDispatcher(loggerFactory);
        }

        public IDisposable On<T>(Action<T> action)
        {
            var subscription = _dispatcher.Subscribe<T>();

            Action<dynamic> handler = message =>
            {
                action((T)message);
            };

            subscription.Received += handler;

            return new DisposableAction(() => subscription.Received -= handler);
        }

        public override void OnMessageReceived<T>(T message)
        {
            _dispatcher.Dispatch(message);
        }

        private bool _disposedValue;
        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    if (_dispatcher != null)
                    {
                        _dispatcher.Dispose();
                        _dispatcher = null;
                    }
                }

                _disposedValue = true;
            }
        }
    }
}