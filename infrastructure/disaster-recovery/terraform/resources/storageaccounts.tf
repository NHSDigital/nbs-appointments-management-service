# Http trigger function app storage account
resource "azurerm_storage_account" "nbs_mya_http_func_storage_account" {
  name                     = "${var.application_short}strgfunc${var.environment}${var.loc}"
  resource_group_name      = data.azurerm_resource_group.nbs_mya_resource_group.name
  location                 = var.location
  # location                 = data.azurerm_resource_group.nbs_mya_resource_group.location
  account_replication_type = var.storage_account_replication_type
  account_tier             = "Standard"
}

# High load trigger function app storage account
resource "azurerm_storage_account" "nbs_mya_high_load_func_storage_account" {
  name                     = "${var.application_short}strghlfunc${var.environment}${var.loc}"
  resource_group_name      = data.azurerm_resource_group.nbs_mya_resource_group.name
  location                 = var.location
  # location                 = data.azurerm_resource_group.nbs_mya_resource_group.location
  account_replication_type = var.storage_account_replication_type
  account_tier             = "Standard"
}

# Synapse workspace app storage account
resource "azurerm_storage_account" "nbs_mya_synapse_workspace_storage_account" {
  count                    = var.cosmos_synapse_enabled ? 1 : 0
  name                     = "${var.application_short}strgsyna${var.environment}${var.loc}"
  resource_group_name      = data.azurerm_resource_group.nbs_mya_resource_group.name
  location                 = var.location
  # location                 = data.azurerm_resource_group.nbs_mya_resource_group.location
  account_replication_type = var.storage_account_replication_type
  account_tier             = "Standard"
}

# ServiceBus trigger function app storage account
resource "azurerm_storage_account" "nbs_mya_servicebus_func_storage_account" {
  name                     = "${var.application_short}strgsbfunc${var.environment}${var.loc}"
  resource_group_name      = data.azurerm_resource_group.nbs_mya_resource_group.name
  location                 = var.location
  #  location                 = data.azurerm_resource_group.nbs_mya_resource_group.location
  account_replication_type = var.storage_account_replication_type
  account_tier             = "Standard"
}

# Timer trigger function app storage account
resource "azurerm_storage_account" "nbs_mya_timer_func_storage_account" {
  name                     = "${var.application_short}strgtmfunc${var.environment}${var.loc}"
  resource_group_name      = data.azurerm_resource_group.nbs_mya_resource_group.name
  location                 = var.location
  # location                 = data.azurerm_resource_group.nbs_mya_resource_group.location
  account_replication_type = var.storage_account_replication_type
  account_tier             = "Standard"
}

## Storage account and container for concurrency leases
resource "azurerm_storage_account" "nbs_mya_leases_storage_account" {
  name                     = "${var.application_short}strglease${var.environment}${var.loc}"
  resource_group_name      = data.azurerm_resource_group.nbs_mya_resource_group.name
  location                 = var.location
  # location                 = data.azurerm_resource_group.nbs_mya_resource_group.location
  account_replication_type = var.storage_account_replication_type
  account_tier             = "Standard"
}

resource "azurerm_storage_container" "nbs_mya_leases_container" {
  name               = "leases"
  storage_account_id = azurerm_storage_account.nbs_mya_leases_storage_account.id
}
