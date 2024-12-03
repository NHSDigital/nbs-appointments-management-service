using MassTransit;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public class ConsoleLogNotifications : IMessageBus
{
    public Task Send<T>(params T[] messages) where T : class
    {
        foreach(var message in messages)
        {
            Console.WriteLine(Json.JsonResponseWriter.Serialize(message));
            ProcessMessage(message);
        }

        return Task.CompletedTask;
    }

    protected virtual void ProcessMessage<T>(T message) { }
}

public class ConsoleLogWithMessageDelivery(IConsumer<UserRolesChanged> userRolesChangedConsumer, IConsumer<BookingMade> bookingMadeConsumer, IConsumer<BookingReminder> bookingReminderConsumer, IConsumer<BookingCancelled> bookingCancelledConsumer, IConsumer<BookingRescheduled> bookingRescheduledConsumer) : ConsoleLogNotifications
{
    protected override void ProcessMessage<T>(T message)
    {
        if(message is UserRolesChanged userRolesChanged)
        {
            userRolesChangedConsumer.Consume(new DummyConsumeContext<UserRolesChanged>() { Message = userRolesChanged });
        }

        if (message is BookingMade bookingMade)
        {
            bookingMadeConsumer.Consume(new DummyConsumeContext<BookingMade>() { Message = bookingMade });
        }

        if (message is BookingRescheduled bookingRescheduled)
        {
            bookingRescheduledConsumer.Consume(new DummyConsumeContext<BookingRescheduled>() { Message = bookingRescheduled });
        }

        if (message is BookingReminder bookingReminder)
        {
            bookingReminderConsumer.Consume(new DummyConsumeContext<BookingReminder>() { Message = bookingReminder });
        }

        if(message is BookingCancelled bookingCancelled)
        {
            bookingCancelledConsumer.Consume(new DummyConsumeContext<BookingCancelled>() {  Message = bookingCancelled });
        }
    }

    private class DummyConsumeContext<T>: ConsumeContext<T> where T : class
    {
        public T Message { get; set; }

        public ReceiveContext ReceiveContext => throw new NotImplementedException();

        public SerializerContext SerializerContext => throw new NotImplementedException();

        public Task ConsumeCompleted => throw new NotImplementedException();

        public IEnumerable<string> SupportedMessageTypes => throw new NotImplementedException();

        public CancellationToken CancellationToken => throw new NotImplementedException();

        public Guid? MessageId => throw new NotImplementedException();

        public Guid? RequestId => throw new NotImplementedException();

        public Guid? CorrelationId => throw new NotImplementedException();

        public Guid? ConversationId => throw new NotImplementedException();

        public Guid? InitiatorId => throw new NotImplementedException();

        public DateTime? ExpirationTime => throw new NotImplementedException();

        public Uri SourceAddress => throw new NotImplementedException();

        public Uri DestinationAddress => throw new NotImplementedException();

        public Uri ResponseAddress => throw new NotImplementedException();

        public Uri FaultAddress => throw new NotImplementedException();

        public DateTime? SentTime => throw new NotImplementedException();

        public Headers Headers => throw new NotImplementedException();

        public HostInfo Host => throw new NotImplementedException();

        public void AddConsumeTask(Task task)
        {
            throw new NotImplementedException();
        }

        public TT AddOrUpdatePayload<TT>(PayloadFactory<TT> addFactory, UpdatePayloadFactory<TT> updateFactory) where TT : class
        {
            throw new NotImplementedException();
        }

        public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
        {
            throw new NotImplementedException();
        }

        public ConnectHandle ConnectSendObserver(ISendObserver observer)
        {
            throw new NotImplementedException();
        }

        public TT GetOrAddPayload<TT>(PayloadFactory<TT> payloadFactory) where TT : class
        {
            throw new NotImplementedException();
        }

        public Task<ISendEndpoint> GetSendEndpoint(Uri address)
        {
            throw new NotImplementedException();
        }

        public bool HasMessageType(Type messageType)
        {
            throw new NotImplementedException();
        }

        public bool HasPayloadType(Type payloadType)
        {
            throw new NotImplementedException();
        }

        public Task NotifyConsumed(TimeSpan duration, string consumerType)
        {
            throw new NotImplementedException();
        }

        public Task NotifyConsumed<TT>(ConsumeContext<TT> context, TimeSpan duration, string consumerType) where TT : class
        {
            throw new NotImplementedException();
        }

        public Task NotifyFaulted(TimeSpan duration, string consumerType, Exception exception)
        {
            throw new NotImplementedException();
        }

        public Task NotifyFaulted<TT>(ConsumeContext<TT> context, TimeSpan duration, string consumerType, Exception exception) where TT : class
        {
            throw new NotImplementedException();
        }

        public Task Publish<TT>(TT message, CancellationToken cancellationToken = default) where TT : class
        {
            throw new NotImplementedException();
        }

        public Task Publish<TT>(TT message, IPipe<PublishContext<TT>> publishPipe, CancellationToken cancellationToken = default) where TT : class
        {
            throw new NotImplementedException();
        }

        public Task Publish<TT>(TT message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where TT : class
        {
            throw new NotImplementedException();
        }

        public Task Publish(object message, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Publish<TT>(object values, CancellationToken cancellationToken = default) where TT : class
        {
            throw new NotImplementedException();
        }

        public Task Publish<TT>(object values, IPipe<PublishContext<TT>> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where TT : class
        {
            throw new NotImplementedException();
        }

        public Task Publish<TT>(object values, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where TT : class
        {
            throw new NotImplementedException();
        }

        public Task Publish<TT>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where TT : class
        {
            throw new NotImplementedException();
        }

        public void Respond<TT>(TT message) where TT : class
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync<TT>(TT message) where TT : class
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync<TT>(TT message, IPipe<SendContext<TT>> sendPipe) where TT : class
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync<TT>(TT message, IPipe<SendContext> sendPipe) where TT : class
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync(object message)
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync(object message, Type messageType)
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync(object message, IPipe<SendContext> sendPipe)
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync(object message, Type messageType, IPipe<SendContext> sendPipe)
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync<TT>(object values) where TT : class
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync<TT>(object values, IPipe<SendContext<TT>> sendPipe) where TT : class
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync<TT>(object values, IPipe<SendContext> sendPipe) where TT : class
        {
            throw new NotImplementedException();
        }

        public bool TryGetMessage<TT>([NotNullWhen(true)] out ConsumeContext<TT> consumeContext) where TT : class
        {
            throw new NotImplementedException();
        }

        public bool TryGetPayload<TT>([NotNullWhen(true)] out TT payload) where TT : class
        {
            throw new NotImplementedException();
        }
    }
}
