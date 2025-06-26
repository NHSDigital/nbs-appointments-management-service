resource "azurerm_storage_data_lake_gen2_filesystem" "nbs_mya_synapse_workspace_gen2_filesystem" {
  count              = var.cosmos_synapse_enabled ? 1 : 0
  name               = "${var.application}-genfs-${var.environment}-${var.loc}"
  storage_account_id = azurerm_storage_account.nbs_mya_synapse_workspace_storage_account[0].id
}

resource "azurerm_synapse_workspace" "nbs_mya_synapse_workspace" {
  count                                = var.cosmos_synapse_enabled ? 1 : 0
  name                                 = "${var.application_short}synw${var.environment}${var.loc}"
  resource_group_name                  = local.resource_group_name
  location                             = var.location
  storage_data_lake_gen2_filesystem_id = azurerm_storage_data_lake_gen2_filesystem.nbs_mya_synapse_workspace_gen2_filesystem[0].id
  sql_administrator_login              = "sqladminuser"
  sql_administrator_login_password     = "S3cur3_this_p0st_p0c!"

  identity {
    type = "SystemAssigned"
  }

  tags = {
    "cost code"     = "PO724/34"
    "created by"    = "Infrastructure Pipeline"
    "created date"  = formatdate("DD/MM/YYYY", timestamp())
    environment     = var.environment
    "product owner" = "Gemma Buchanan"
    "requested by"  = "Paul Tallet"
    service-product = "National Booking Service"
    team            = "NBS"
  }
}

resource "azurerm_synapse_integration_runtime_azure" "nbs_mya_synapse_runtime" {
  count                = var.cosmos_synapse_enabled ? 1 : 0
  name                 = "${var.application_short}synrun${var.environment}${var.loc}"
  synapse_workspace_id = azurerm_synapse_workspace.nbs_mya_synapse_workspace[0].id
  location             = var.location
}

resource "azurerm_synapse_linked_service" "nbs_mya_synapse_linked_service" {
  count                = var.cosmos_synapse_enabled ? 1 : 0
  name                 = "${var.application_short}synls${var.environment}${var.loc}"
  synapse_workspace_id = azurerm_synapse_workspace.nbs_mya_synapse_workspace[0].id
  type                 = "CosmosDb"
  type_properties_json = <<JSON
{
  "connectionString": "AccountEndpoint=${azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].endpoint};AccountKey=${azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].primary_key};"
}
JSON
  integration_runtime {
    name = azurerm_synapse_integration_runtime_azure.nbs_mya_synapse_runtime[0].name
  }
}

resource "azurerm_synapse_spark_pool" "nbs_mya_synapse_spark_pool" {
  count                = var.cosmos_synapse_enabled ? 1 : 0
  name                 = "myasyns${var.environment}"
  synapse_workspace_id = azurerm_synapse_workspace.nbs_mya_synapse_workspace[0].id
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

resource "azurerm_synapse_role_assignment" "nbs_mya_synapse_role_assignment" {
  count                = var.cosmos_synapse_enabled ? 1 : 0
  synapse_workspace_id = azurerm_synapse_workspace.nbs_mya_synapse_workspace[0].id
  role_name            = "Synapse Administrator"
  principal_id         = "06394083-2cba-4f66-b56d-7de6e0f5db30"
  principal_type       = "User"
}