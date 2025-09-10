resource "azurerm_container_app_environment" "nbs_mya_container_enviroment" {
  count                      = var.create_data_extracts ? 1 : 0 
  name                       = "${var.application}-cae-${var.environment}-${var.loc}"
  location                   = var.location
  resource_group_name        = local.resource_group_name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.nbs_mya_log_analytics_workspace.id
}

resource "azurerm_container_app_job" "nbs_mya_booking_extracts_job" {
  count                        = var.create_data_extracts ? 1 : 0 
  name                         = "${var.application}-bookjob-${var.environment}-${var.loc}"
  resource_group_name          = local.resource_group_name
  location                     = var.location
  container_app_environment_id = azurerm_container_app_environment.nbs_mya_container_enviroment[0].id

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
  secret {
    name  = "container-cosmos-password"
    value = var.cosmos_token != "" ? var.cosmos_token : azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].primary_key
  }
  secret {
    name  = "blob-connection-string"
    value = var.data_extract_file_sender_options_type == "blob" ? azurerm_storage_account.nbs_mya_container_app_storage_account[0].primary_blob_connection_string : ""
  }
  secret {
    name  = "mesh-mailbox-password"
    value = var.mesh_authorization_options_mailbox_password
  }
  secret {
    name  = "mesh-shared-key"
    value = var.mesh_authorization_options_shared_key
  }
  secret {
    name  = "key-vault-secret"
    value = var.keyvault_client_secret
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
        value = var.cosmos_endpoint != "" ? var.cosmos_endpoint : azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].endpoint
      }
      env {
        name  = "COSMOS_TOKEN"
        secret_name = "container-cosmos-password"
      }
      env {
        name = "FileSenderOptions__Type"
        value = var.data_extract_file_sender_options_type
      }
      env {
        name  = "BlobStorageConnectionString"
        secret_name = "blob-connection-string"
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
        secret_name = "mesh-mailbox-password"
      }
      env {
        name  = "MeshAuthorizationOptions__SharedKey"
        secret_name = "mesh-shared-key"
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
        secret_name = "key-vault-secret"
      }
    }
  }

  identity {
    type = "SystemAssigned"
  }
}


resource "azurerm_container_app_job" "nbs_mya_capacity_extracts_job" {
  count                        = var.create_data_extracts ? 1 : 0 
  name                         = "${var.application}-capjob-${var.environment}-${var.loc}"
  resource_group_name          = local.resource_group_name
  location                     = var.location
  container_app_environment_id = azurerm_container_app_environment.nbs_mya_container_enviroment[0].id

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
  secret {
    name  = "container-cosmos-password"
    value = var.cosmos_token != "" ? var.cosmos_token : azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].primary_key
  }
  secret {
    name  = "blob-connection-string"
    value = var.data_extract_file_sender_options_type == "blob" ? azurerm_storage_account.nbs_mya_container_app_storage_account[0].primary_blob_connection_string : ""
  }
  secret {
    name  = "mesh-mailbox-password"
    value = var.mesh_authorization_options_mailbox_password
  }
  secret {
    name  = "mesh-shared-key"
    value = var.mesh_authorization_options_shared_key
  }
  secret {
    name  = "key-vault-secret"
    value = var.keyvault_client_secret
  }

  registry {
    server   = var.container_registry_server_url
    username = var.container_registry_username
    password_secret_name = "container-registry-password"
  }

  template {
    container {
      name   = "nbs-mya-capacity-extracts"
      image  = "${var.container_registry_server_url}/capacity-extract:${var.build_number}"
      cpu    = 1
      memory = "2Gi"
      env {
        name  = "COSMOS_ENDPOINT"
        value = var.cosmos_endpoint != "" ? var.cosmos_endpoint : azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].endpoint
      }
      env {
        name  = "COSMOS_TOKEN"
        secret_name = "container-cosmos-password"
      }
      env {
        name = "FileSenderOptions__Type"
        value = var.data_extract_file_sender_options_type
      }
      env {
        name  = "BlobStorageConnectionString"
        secret_name = "blob-connection-string"
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
        secret_name = "mesh-mailbox-password"
      }
      env {
        name  = "MeshAuthorizationOptions__SharedKey"
        secret_name = "mesh-shared-key"
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
        secret_name = "key-vault-secret"
      }
    }
  }

  identity {
    type = "SystemAssigned"
  }
}
