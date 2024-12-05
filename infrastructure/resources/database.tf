resource "azurerm_cosmosdb_account" "nbs_mya_cosmos_db" {
  name                       = "${var.application}-cdb-${var.environment}-${var.loc}"
  location                   = data.azurerm_resource_group.nbs_mya_resource_group.location
  resource_group_name        = data.azurerm_resource_group.nbs_mya_resource_group.name
  offer_type                 = "Standard"
  kind                       = "GlobalDocumentDB"
  automatic_failover_enabled = var.cosmos_automatic_failover_enabled
  dynamic "geo_location" {
    for_each = var.cosmos_geo_locations
    content {
      location          = geo_location.value["location"]
      failover_priority = geo_location.value["failover_priority"]
      zone_redundant    = geo_location.value["zone_redundant"]
    }
  }
  dynamic "capabilities" {
    for_each = var.cosmos_capabilities
    content {
      name = capabilities.value["name"]
    }
  }
  consistency_policy {
    consistency_level = "Session"
  }
}

resource "azurerm_cosmosdb_sql_database" "nbs_appts_database" {
  name                = "appts"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  account_name        = azurerm_cosmosdb_account.nbs_mya_cosmos_db.name
}

resource "azurerm_cosmosdb_sql_container" "nbs_mya_booking_container" {
  name                = "booking_data"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  account_name        = azurerm_cosmosdb_account.nbs_mya_cosmos_db.name
  database_name       = azurerm_cosmosdb_sql_database.nbs_appts_database.name
  partition_key_path  = "/site"
  dynamic "autoscale_settings" {
    for_each = var.cosmos_booking_autoscale_settings
    content {
      max_throughput = autoscale_settings.value["max_throughput"]
    }
  }
}

resource "azurerm_cosmosdb_sql_container" "nbs_mya_index_container" {
  name                = "index_data"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  account_name        = azurerm_cosmosdb_account.nbs_mya_cosmos_db.name
  database_name       = azurerm_cosmosdb_sql_database.nbs_appts_database.name
  partition_key_path  = "/docType"

  dynamic "autoscale_settings" {
    for_each = var.cosmos_index_autoscale_settings
    content {
      max_throughput = autoscale_settings.value["max_throughput"]
    }
  }
}
