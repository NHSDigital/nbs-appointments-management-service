using System.Linq.Expressions;

namespace Nhs.Appointments.Core;

public interface IBookingsDocumentStore 
{
    Task InsertAsync(Booking booking);
    Task<IEnumerable<Booking>> GetInDateRangeAsync(string site, DateTime from, DateTime to);
    Task<Booking> GetByReferenceOrDefaultAsync(string bookingReference);
    Task<IEnumerable<Booking>> GetByNhsNumberAsync(string nhsNumber);
    Task<bool> UpdateStatus(string bookingReference, string status);
    IDocumentUpdate<Booking> BeginUpdate(string site, string reference);    
}

public interface ISiteConfigurationStore
{
    Task ReplaceOrCreate(SiteConfiguration siteConfiguration);
    Task<SiteConfiguration> GetAsync(string site);
    Task<SiteConfiguration> GetOrDefault(string site);
    Task AssignPrefix(string site, int prefix);
}

public interface IUserSiteAssignmentStore
{
    Task<IEnumerable<UserAssignment>> GetUserAssignedSites(string userId);
}

public interface IRolesStore
{
    Task<IEnumerable<Role>> GetRoles();
}

public interface IDocumentUpdate<TModel>
{
    IDocumentUpdate<TModel> UpdateProperty<TProp>(Expression<Func<TModel, TProp>> prop, TProp val);
    Task ApplyAsync();
}
