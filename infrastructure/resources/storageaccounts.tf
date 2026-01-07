# Http trigger function app storage account
resource "azurerm_storage_account" "nbs_mya_http_func_storage_account" {
  name                     = "${var.application_short}strgfunc${var.environment}${var.loc}"
  resource_group_name      = local.resource_group_name
  location                 = var.location
  account_replication_type = var.storage_account_replication_type
  account_tier             = "Standard"
}

# High load trigger function app storage account
resource "azurerm_storage_account" "nbs_mya_high_load_func_storage_account" {
  count                    = var.create_high_load_function_app ? 1 : 0
  name                     = "${var.application_short}strghlfunc${var.environment}${var.loc}"
  resource_group_name      = local.resource_group_name
  location                 = var.location
  account_replication_type = var.storage_account_replication_type
  account_tier             = "Standard"
}

# ServiceBus trigger function app storage account
resource "azurerm_storage_account" "nbs_mya_servicebus_func_storage_account" {
  name                     = "${var.application_short}strgsbfunc${var.environment}${var.loc}"
  resource_group_name      = local.resource_group_name
  location                 = var.location
  account_replication_type = var.storage_account_replication_type
  account_tier             = "Standard"
}

# Timer trigger function app storage account
resource "azurerm_storage_account" "nbs_mya_timer_func_storage_account" {
  name                     = "${var.application_short}strgtmfunc${var.environment}${var.loc}"
  resource_group_name      = local.resource_group_name
  location                 = var.location
  account_replication_type = var.storage_account_replication_type
  account_tier             = "Standard"
}

# High load trigger function app storage account
resource "azurerm_storage_account" "nbs_mya_container_app_storage_account" {
  count                    = var.data_extract_file_sender_options_type == "blob" ? 1 : 0
  name                     = "${var.application_short}strgextract${var.environment}${var.loc}"
  resource_group_name      = local.resource_group_name
  location                 = var.location
  account_replication_type = var.storage_account_replication_type
  account_tier             = "Standard"
}

## Storage account and container for concurrency leases
resource "azurerm_storage_account" "nbs_mya_leases_storage_account" {
  name                     = "${var.application_short}strglease${var.environment}${var.loc}"
  resource_group_name      = local.resource_group_name
  location                 = var.location
  account_replication_type = var.storage_account_replication_type
  account_tier             = "Standard"
}

resource "azurerm_storage_container" "nbs_mya_leases_container" {
  name               = "leases"
  storage_account_id = azurerm_storage_account.nbs_mya_leases_storage_account.id
}

# Audit storage account
resource "azurerm_storage_account" "nbs_mya_audit_storage_account" {
  name                     = "${var.application_short}strgaudit${var.environment}${var.loc}"
  resource_group_name      = local.resource_group_name
  location                 = var.location
  account_replication_type = var.storage_account_replication_type
  account_tier             = "Standard"
}

# Audit lifecycle policy to move to Archive tier
resource "azurerm_storage_management_policy" "audit_lifecycle_policy" {
  storage_account_id = azurerm_storage_account.nbs_mya_audit_storage_account.id

  rule {
    name    = "ArchiveOldAudits"
    enabled = true
    filters {
      blob_types   = ["blockBlob"]
    }
    actions {
      base_blob {
        tier_to_archive_after_days_since_modification_greater_than = var.auditor_archive_after_days
      }
    }
  }
}
