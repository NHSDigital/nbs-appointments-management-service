using MassTransit;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public class NullMessageBus : IMessageBus
{

    public Task Send<T>(params T[] messages) where T : class => Task.CompletedTask;

}

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

        public TPayload AddOrUpdatePayload<TPayload>(PayloadFactory<TPayload> addFactory, UpdatePayloadFactory<TPayload> updateFactory) where TPayload : class
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

        public TPayload GetOrAddPayload<TPayload>(PayloadFactory<TPayload> payloadFactory) where TPayload : class
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

        public Task NotifyConsumed<TPayload>(ConsumeContext<TPayload> context, TimeSpan duration, string consumerType) where TPayload : class
        {
            throw new NotImplementedException();
        }

        public Task NotifyFaulted(TimeSpan duration, string consumerType, Exception exception)
        {
            throw new NotImplementedException();
        }

        public Task NotifyFaulted<TPayload>(ConsumeContext<TPayload> context, TimeSpan duration, string consumerType, Exception exception) where TPayload : class
        {
            throw new NotImplementedException();
        }

        public Task Publish<TPayload>(TPayload message, CancellationToken cancellationToken = default) where TPayload : class
        {
            throw new NotImplementedException();
        }

        public Task Publish<TPayload>(TPayload message, IPipe<PublishContext<TPayload>> publishPipe, CancellationToken cancellationToken = default) where TPayload : class
        {
            throw new NotImplementedException();
        }

        public Task Publish<TPayload>(TPayload message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where TPayload : class
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

        public Task Publish<TPayload>(object values, CancellationToken cancellationToken = default) where TPayload : class
        {
            throw new NotImplementedException();
        }

        public Task Publish<TPayload>(object values, IPipe<PublishContext<TPayload>> publishPipe, CancellationToken cancellationToken = new CancellationToken()) where TPayload : class
        {
            throw new NotImplementedException();
        }

        public Task Publish<TPayload>(object values, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where TPayload : class
        {
            throw new NotImplementedException();
        }

        public Task Publish<TPayload>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where TPayload : class
        {
            throw new NotImplementedException();
        }

        public void Respond<TPayload>(TPayload message) where TPayload : class
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync<TPayload>(TPayload message) where TPayload : class
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync<TPayload>(TPayload message, IPipe<SendContext<TPayload>> sendPipe) where TPayload : class
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync<TPayload>(TPayload message, IPipe<SendContext> sendPipe) where TPayload : class
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

        public Task RespondAsync<TPayload>(object values) where TPayload : class
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync<TPayload>(object values, IPipe<SendContext<TPayload>> sendPipe) where TPayload : class
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync<TPayload>(object values, IPipe<SendContext> sendPipe) where TPayload : class
        {
            throw new NotImplementedException();
        }

        public bool TryGetMessage<TPayload>([NotNullWhen(true)] out ConsumeContext<TPayload> consumeContext) where TPayload : class
        {
            throw new NotImplementedException();
        }

        public bool TryGetPayload<TPayload>([NotNullWhen(true)] out TPayload payload) where TPayload : class
        {
            throw new NotImplementedException();
        }
    }
}
