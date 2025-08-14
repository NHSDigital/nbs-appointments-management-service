resource "azurerm_container_app_job" "nbs_mya_bookings_data_extract_job" {
  name                         = "${var.application}-cabookjob-${var.environment}-${var.loc}"
  resource_group_name          = local.resource_group_name
  location                     = var.location
  container_app_environment_id = azurerm_container_app_environment.nbs_mya_container_env.id

  replica_timeout_in_seconds = var.data_extract_timeout    
  replica_retry_limit        = var.data_extract_retry_limit    

  schedule_trigger_config {
    cron_expression = var.data_extract_schedule
    parallelism     = 1
  }

  template {
    container {
      name   = "bookings-data-extract"
      image  = "myregistry.azurecr.io/myconsoleapp:latest"
      cpu    = 1
      memory = "2Gi"
      env {
        name  = "COSMOS_ENDPOINT"
        value = azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].endpoint
      }
      env {
        name  = "COSMOS_TOKEN"
        value = azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].primary_key
      }
      env {
        name  = "MESH_MAILBOX_DESTINATION"
        value = var.mesh_bookings_mailbox_destination
      }
      env {
        name  = "MESH_WORKFLOW"
        value = var.mesh_bookings_mailbox_workflow
      }
      env {
        name  = "MeshClientOptions__BaseUrl"
        value = var.mesh_client_options_base_url
      }
      env {
        name  = "MeshAuthorizationOptions__MailboxId"
        value = var.mesh_authorization_options_mailbox_id
      }
      env {
        name  = "MeshAuthorizationOptions__MailboxPassword"
        value = var.mesh_authorization_options_mailbox_password
      }
      env {
        name  = "MeshAuthorizationOptions__SharedKey"
        value = var.mesh_authorization_options_shared_key
      }
    }
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_container_app_job" "nbs_mya_capacity_data_extract_job" {
  name                         = "${var.application}-cacapjob-${var.environment}-${var.loc}"
  resource_group_name          = local.resource_group_name
  location                     = var.location
  container_app_environment_id = azurerm_container_app_environment.nbs_mya_container_env.id

  replica_timeout_in_seconds = var.data_extract_timeout
  replica_retry_limit        = var.data_extract_retry_limit

  schedule_trigger_config {
    cron_expression = var.data_extract_schedule
    parallelism     = 1
  }

  template {
    container {
      name   = "capacity_data_extract"
      image  = "myregistry.azurecr.io/myconsoleapp:latest"
      cpu    = 1
      memory = "2Gi"
      env {
        name  = "COSMOS_ENDPOINT"
        value = azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].endpoint
      }
      env {
        name  = "COSMOS_TOKEN"
        value = azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].primary_key
      }
      env {
        name  = "MESH_MAILBOX_DESTINATION"
        value = var.mesh_capacity_mailbox_destination
      }
      env {
        name  = "MESH_WORKFLOW"
        value = var.mesh_capacity_mailbox_workflow
      }
      env {
        name  = "MeshClientOptions__BaseUrl"
        value = var.mesh_client_options_base_url
      }
      env {
        name  = "MeshAuthorizationOptions__MailboxId"
        value = var.mesh_authorization_options_mailbox_id
      }
      env {
        name  = "MeshAuthorizationOptions__MailboxPassword"
        value = var.mesh_authorization_options_mailbox_password
      }
      env {
        name  = "MeshAuthorizationOptions__SharedKey"
        value = var.mesh_authorization_options_shared_key
      }
    }
  }

  identity {
    type = "SystemAssigned"
  }
}
