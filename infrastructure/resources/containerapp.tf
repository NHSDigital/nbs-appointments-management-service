resource "azurerm_log_analytics_workspace" "nbs_mya_container_analytics_workspace" {
  name                = "${var.application}-caws-${var.environment}-${var.loc}"
  location            = var.location
  resource_group_name = local.resource_group_name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_container_app_environment" "nbs_mya_container_enviroment" {
  name                       = "${var.application}-cae-${var.environment}-${var.loc}"
  location                   = var.location
  resource_group_name        = local.resource_group_name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.nbs_mya_container_analytics_workspace.id
}

resource "azurerm_container_app_job" "nbs_mya_booking_extracts_job" {
  name                         = "${var.application}-bookjob-${var.environment}-${var.loc}"
  resource_group_name          = local.resource_group_name
  location                     = var.location
  container_app_environment_id = azurerm_container_app_environment.nbs_mya_container_enviroment.id

  replica_timeout_in_seconds = var.data_extract_timeout    
  replica_retry_limit        = var.data_extract_retry_limit    

  schedule_trigger_config {
    cron_expression = var.data_extract_schedule
    parallelism     = 1
  }

  secret {
    name  = "container-registry-password"
    value = var.container_registry_password
  }

  registry {
    server   = var.container_registry_server_url
    username = var.container_registry_username
    password_secret_name = "container-registry-password"
  }

  template {
    container {
      name   = "nbs-mya-booking-extracts"
      image  = "${var.container_registry_server_url}/booking-extract:${var.build_number}"
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
        name = "FileSenderOptions__Type"
        value = var.data_extract_file_sender_options_type
      }
      env {
        name  = "BlobStorageConnectionString"
        value = var.data_extract_file_sender_options_type == "blob" ? azurerm_storage_account.nbs_mya_container_app_storage_account[0].primary_blob_connection_string : ""
      }
      env {
        name  = "MESH_MAILBOX_DESTINATION"
        value = var.mesh_mailbox_destination
      }
      env {
        name  = "MESH_WORKFLOW"
        value = var.mesh_mailbox_workflow_booking
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
      env {
        name = "MeshAuthorizationOptions__CertificateName"
        value = var.mesh_authorization_options_certificate_name
      }
      env {
        name = "KeyVault__KeyVaultName"
        value = var.keyvault_Name
      }
      env {
        name = "KeyVault__TenantId"
        value = var.keyvault_tenant_id
      }
      env {
        name = "KeyVault__ClientId"
        value = var.keyvault_client_id
      }
      env {
        name = "KeyVault__ClientSecret"
        value = var.keyvault_client_secret
      }
    }
  }

  identity {
    type = "SystemAssigned"
  }
}


resource "azurerm_container_app_job" "nbs_mya_capacity_extracts_job" {
  name                         = "${var.application}-capjob-${var.environment}-${var.loc}"
  resource_group_name          = local.resource_group_name
  location                     = var.location
  container_app_environment_id = azurerm_container_app_environment.nbs_mya_container_enviroment.id

  replica_timeout_in_seconds = var.data_extract_timeout    
  replica_retry_limit        = var.data_extract_retry_limit    

  schedule_trigger_config {
    cron_expression = var.data_extract_schedule
    parallelism     = 1
  }

  secret {
    name  = "container-registry-password"
    value = var.container_registry_password
  }

  registry {
    server   = var.container_registry_server_url
    username = var.container_registry_username
    password_secret_name = "container-registry-password"
  }

  template {
    container {
      name   = "${var.container_registry_server_url}/nbs-mya-capacity-extracts"
      image  = "capacity-extract:${var.build_number}"
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
        name = "FileSenderOptions__Type"
        value = var.data_extract_file_sender_options_type
      }
      env {
        name  = "BlobStorageConnectionString"
        value = var.data_extract_file_sender_options_type == "blob" ? azurerm_storage_account.nbs_mya_container_app_storage_account[0].primary_blob_connection_string : ""
      }
      env {
        name  = "MESH_MAILBOX_DESTINATION"
        value = var.mesh_mailbox_destination
      }
      env {
        name  = "MESH_WORKFLOW"
        value = var.mesh_mailbox_workflow_capacity
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
      env {
        name = "MeshAuthorizationOptions__CertificateName"
        value = var.mesh_authorization_options_certificate_name
      }
      env {
        name = "KeyVault__KeyVaultName"
        value = var.keyvault_Name
      }
      env {
        name = "KeyVault__TenantId"
        value = var.keyvault_tenant_id
      }
      env {
        name = "KeyVault__ClientId"
        value = var.keyvault_client_id
      }
      env {
        name = "KeyVault__ClientSecret"
        value = var.keyvault_client_secret
      }
    }
  }

  identity {
    type = "SystemAssigned"
  }
}
