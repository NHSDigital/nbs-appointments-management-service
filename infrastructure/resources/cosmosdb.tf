resource "azurerm_cosmosdb_account" "nbs_mya_cosmos_db" {
  name                       = "${var.application}-cdb-${var.environment}-${var.loc}"
  location                   = var.location
  resource_group_name        = data.azurerm_resource_group.nbs_mya_resource_group.name
  offer_type                 = "Standard"
  kind                       = "GlobalDocumentDB"
  automatic_failover_enabled = var.cosmos_automatic_failover_enabled
  analytical_storage_enabled = var.cosmos_synapse_enabled
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
  backup {
    type = "Continuous"
    tier = "Continuous7Days"
  }
}

resource "azurerm_cosmosdb_sql_database" "nbs_appts_database" {
  name                = "appts"
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  account_name        = azurerm_cosmosdb_account.nbs_mya_cosmos_db.name
}

resource "azurerm_cosmosdb_sql_container" "nbs_mya_booking_container" {
  name                   = "booking_data"
  resource_group_name    = data.azurerm_resource_group.nbs_mya_resource_group.name
  account_name           = azurerm_cosmosdb_account.nbs_mya_cosmos_db.name
  database_name          = azurerm_cosmosdb_sql_database.nbs_appts_database.name
  partition_key_paths    = ["/site"]
  analytical_storage_ttl = var.cosmos_synapse_enabled ? -1 : 0

  dynamic "autoscale_settings" {
    for_each = var.cosmos_booking_autoscale_settings
    content {
      max_throughput = autoscale_settings.value["max_throughput"]
    }
  }
  indexing_policy {
    indexing_mode = "consistent"
    included_path {
      path = "/*"
    }
    excluded_path {
      path = "/_etag/?"
    }
    excluded_path {
      path = "/created/?"
    }
    excluded_path {
      path = "/by/?"
    }
    excluded_path {
      path = "/template/*"
    }
    excluded_path {
      path = "/to/?"
    }
    excluded_path {
      path = "/sessions/*"
    }
    excluded_path {
      path = "/reference/?"
    }
    excluded_path {
      path = "/duration/?"
    }
    excluded_path {
      path = "/service/?"
    }
    excluded_path {
      path = "/status/?"
    }
    excluded_path {
      path = "/attendeeDetails/*"
    }
    excluded_path {
      path = "/contactDetails/*"
    }
    excluded_path {
      path = "/additionalData/*"
    }
    excluded_path {
      path = "/reminderSent/?"
    }
  }
}

resource "azurerm_cosmosdb_sql_container" "nbs_mya_core_container" {
  name                   = "core_data"
  resource_group_name    = data.azurerm_resource_group.nbs_mya_resource_group.name
  account_name           = azurerm_cosmosdb_account.nbs_mya_cosmos_db.name
  database_name          = azurerm_cosmosdb_sql_database.nbs_appts_database.name
  partition_key_paths    = ["/docType"]
  analytical_storage_ttl = var.cosmos_synapse_enabled ? -1 : 0

  dynamic "autoscale_settings" {
    for_each = var.cosmos_core_autoscale_settings
    content {
      max_throughput = autoscale_settings.value["max_throughput"]
    }
  }
}

resource "azurerm_cosmosdb_sql_container" "nbs_mya_index_container" {
  name                   = "index_data"
  resource_group_name    = data.azurerm_resource_group.nbs_mya_resource_group.name
  account_name           = azurerm_cosmosdb_account.nbs_mya_cosmos_db.name
  database_name          = azurerm_cosmosdb_sql_database.nbs_appts_database.name
  partition_key_paths    = ["/docType"]
  analytical_storage_ttl = var.cosmos_synapse_enabled ? -1 : 0

  dynamic "autoscale_settings" {
    for_each = var.cosmos_index_autoscale_settings
    content {
      max_throughput = autoscale_settings.value["max_throughput"]
    }
  }
}

resource "azurerm_cosmosdb_sql_container" "nbs_mya_audit_container" {
  name                   = "audit_data"
  resource_group_name    = data.azurerm_resource_group.nbs_mya_resource_group.name
  account_name           = azurerm_cosmosdb_account.nbs_mya_cosmos_db.name
  database_name          = azurerm_cosmosdb_sql_database.nbs_appts_database.name
  partition_key_paths    = ["/user"]
  analytical_storage_ttl = var.cosmos_synapse_enabled ? -1 : 0

  dynamic "autoscale_settings" {
    for_each = var.cosmos_audit_autoscale_settings
    content {
      max_throughput = autoscale_settings.value["max_throughput"]
    }
  }
}
