# Concurrency

## Make Booking

In order to prevent concurrent calls to make booking from booking the same time slot, and causing and overbooked block of time the system implements a leasing mechanism. Leases are acquired at a site level, meaning that only one booking can be processed for a given site at a given instance in time. The leasing mechanism using Azure Blob Leases, with a blob existing for each site (note the blob will be created on demand). The connection string to the blob storage account is configured with the LEASE_MANAGER_CONNECTION application setting. This can be set to `local` to allow local testing using an in memory lease manager. Consideration should be given to this setting when deploying into multiple regions, as this could introduce a loop whole if the function app in the separate regions are not using the same storage account for leasing. This will not be an issue during the alpha development as it will only be deployed to a single region.
