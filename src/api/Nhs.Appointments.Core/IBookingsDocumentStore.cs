﻿using System.Linq.Expressions;

namespace Nhs.Appointments.Core;

public interface IBookingsDocumentStore 
{
    Task InsertAsync(Booking booking);
    Task<IEnumerable<Booking>> GetInDateRangeAsync(DateTime from, DateTime to, string site);
    Task<IEnumerable<Booking>> GetCrossSiteAsync(DateTime from, DateTime to);
    Task<Booking> GetByReferenceOrDefaultAsync(string bookingReference);
    Task<IEnumerable<Booking>> GetByNhsNumberAsync(string nhsNumber);
    Task<bool> UpdateStatus(string bookingReference, string status);
    IDocumentUpdate<Booking> BeginUpdate(string site, string reference);
    Task SetReminderSent(string bookingReference, string site);
}

public interface IRolesStore
{
    Task<IEnumerable<Role>> GetRoles();
}

public interface INotificationConfigurationStore
{
    Task<NotificationConfiguration> GetNotificationConfiguration(string eventType);
    Task<NotificationConfiguration> GetNotificationConfigurationForService(string eventType, string serviceId);
}

public interface IDocumentUpdate<TModel>
{
    IDocumentUpdate<TModel> UpdateProperty<TProp>(Expression<Func<TModel, TProp>> prop, TProp val);
    Task ApplyAsync();
}
