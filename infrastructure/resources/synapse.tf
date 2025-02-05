resource "azurerm_storage_data_lake_gen2_filesystem" "nbs_mya_synapse_workspace_gen2_filesystem" {
  count              = var.cosmos_synapse_enabled ? 1 : 0
  name               = "${var.application}-genfs-${var.environment}-${var.loc}"
  storage_account_id = azurerm_storage_account.nbs_mya_synapse_workspace_storage_account.id
}

resource "azurerm_synapse_workspace" "nbs_mya_synapse_workspace" {
  count                                = var.cosmos_synapse_enabled ? 1 : 0
  name                                 = "${var.application_short}synw${var.environment}${var.loc}"
  resource_group_name                  = data.azurerm_resource_group.nbs_mya_resource_group.name
  location                             = data.azurerm_resource_group.nbs_mya_resource_group.location
  storage_data_lake_gen2_filesystem_id = azurerm_storage_data_lake_gen2_filesystem.nbs_mya_synapse_workspace_gen2_filesystem.id
  sql_administrator_login              = "sqladminuser"
  sql_administrator_login_password     = "secure_this_post_poc"

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_synapse_firewall_rule" "nbs_mya_synapse_firewall" {
  count                = var.cosmos_synapse_enabled ? 1 : 0
  name                 = "allowAll"
  synapse_workspace_id = azurerm_synapse_workspace.nbs_mya_synapse_workspace.id
  start_ip_address     = "0.0.0.0"
  end_ip_address       = "255.255.255.255"
}

resource "azurerm_synapse_integration_runtime_azure" "nbs_mya_synapse_runtime" {
  count                = var.cosmos_synapse_enabled ? 1 : 0
  name                 = "${var.application_short}synrun${var.environment}${var.loc}"
  synapse_workspace_id = azurerm_synapse_workspace.nbs_mya_synapse_workspace.id
  location             = data.azurerm_resource_group.nbs_mya_resource_group.location
}

resource "azurerm_synapse_linked_service" "example" {
  count                = var.cosmos_synapse_enabled ? 1 : 0
  name                 = "${var.application_short}synls${var.environment}${var.loc}"
  synapse_workspace_id = azurerm_synapse_workspace.nbs_mya_synapse_workspace.id
  type                 = "CosmosDb"
  type_properties_json = <<JSON
{
  "connectionString": "${azurerm_cosmosdb_account.nbs_mya_cosmos_db.connection_strings[0]}Database=${azurerm_cosmosdb_sql_database.nbs_appts_database.name}"
}
JSON
  integration_runtime {
    name = azurerm_synapse_integration_runtime_azure.example.name
  }

  depends_on = [
    azurerm_synapse_firewall_rule.nbs_mya_synapse_firewall,
  ]
}

resource "azurerm_synapse_spark_pool" "nbs_mya_synapse_spark_pool" {
  count                = var.cosmos_synapse_enabled ? 1 : 0
  name                 = "${var.application_short}syns${var.environment}${var.loc}"
  synapse_workspace_id = azurerm_synapse_workspace.nbs_mya_synapse_workspace.id
  node_size_family     = "MemoryOptimized"
  node_size            = "Small"
  cache_size           = 100

  auto_scale {
    max_node_count = 3
    min_node_count = 3
  }

  auto_pause {
    delay_in_minutes = 15
  }

  library_requirement {
    content  = <<EOF
appnope==0.1.0
beautifulsoup4==4.6.3
EOF
    filename = "requirements.txt"
  }

  spark_config {
    content  = <<EOF
spark.shuffle.spill                true
EOF
    filename = "config.txt"
  }

  spark_version = 3.4
}