using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nhs.Appointments.Jobs.ChangeFeed;

public abstract class DataProcessor
{
    private readonly ILogger<DataProcessor> _logger;
    private readonly IOptions<DataFilterOptions> _options;
    private HashSet<string>? _filteredSitePrefixes;
    private HashSet<string>? _filteredDocumentTypes;
    
    protected DataProcessor(IOptions<DataFilterOptions> options, ILogger<DataProcessor> logger)
    {
        _logger = logger;
        _options = options;

        SetupDocumentTypeConfiguration();
        SetupSiteConfiguration();
    }

    private void SetupDocumentTypeConfiguration()
    {
        var docTypes = _options.Value.DocumentTypes;

        if (docTypes is null || docTypes.Count == 0)
        {
            //allow all document types
            _filteredDocumentTypes = null; 
            _logger.LogInformation("All document types allowed.");
        }
        else
        {
            _filteredDocumentTypes = new HashSet<string>(docTypes, StringComparer.OrdinalIgnoreCase);
            _logger.LogInformation("Allowed document types: {DocTypes}.", string.Join(", ", _filteredDocumentTypes));
        }
    }
    
    private void SetupSiteConfiguration()
    {
        var sites = _options.Value.Sites;
        
        if (sites is null || sites.Count == 0)
        {
            //allow all sites
            _filteredSitePrefixes = null; 
            _logger.LogInformation("All sites allowed.");
        }
        else
        {
            _filteredSitePrefixes = new HashSet<string>(sites, StringComparer.OrdinalIgnoreCase);
            _logger.LogInformation("Allowed site prefixes: {Sites}.", string.Join(", ", _filteredSitePrefixes));
        }
    }

    protected virtual bool CanProcessSite(string? site)
    {
        if (_filteredSitePrefixes is null)
        {
            return true;
        }
        
        if (site is null)
        {
            _logger.LogInformation("Site is null.");
            return true;
        }
        
        //startsWith logic, as some tests append test identifier suffixes
        var canProcess = _filteredSitePrefixes.Any(site.StartsWith);

        if (!canProcess)
        {
            _logger.LogInformation("Site '{Site}' is filtered out.", site);
        }
        
        return canProcess;
    }

    protected virtual bool CanProcessDocumentType(string? docType)
    {
        if (_filteredDocumentTypes is null)
        {
            return true;
        }
        
        if (docType is null)
        {
            _logger.LogInformation("Document Type is null.");
            return true;
        }
        
        var canProcess = _filteredDocumentTypes.Contains(docType);

        if (!canProcess)
        {
            _logger.LogInformation("Document Type '{DocType}' is filtered out.", docType);
        }
        
        return canProcess;
    }
}
