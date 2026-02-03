resource "azurerm_container_app_environment" "nbs_mya_container_enviroment" {
  name                       = "${var.application}-cae-${var.environment}-${var.loc}"
  location                   = var.location
  resource_group_name        = local.resource_group_name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.nbs_mya_log_analytics_workspace.id
}

resource "azurerm_container_app" "nbs_mya_splunk_otel_collector" {
  name                         = "${var.application}-sotel-${var.environment}-${var.loc}"
  container_app_environment_id = azurerm_container_app_environment.nbs_mya_container_enviroment.id
  resource_group_name          = local.resource_group_name
  revision_mode                = "Single"
  
  secret {
    name  = "container-registry-password"
    value = var.container_registry_password
  }
  secret {
    name  = "splunk-hec-token"
    value = var.splunk_hec_token
  }

  registry {
    server   = var.container_registry_server_url
    username = var.container_registry_username
    password_secret_name = "container-registry-password"
  }

  template {
    container {
      name   = "nbs-mya-splunk-otel-collector"
      image  = "${var.container_registry_server_url}/otel/splunk:${var.splunk_otel_image_version}"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name = "SPLUNK_HEC_TOKEN"
        secret_name = "splunk-hec-token"
      }
      env {
        name = "SPLUNK_HOST_URL"
        value = var.splunk_host_url
      }
      env {
        name = "SPLUNK_LOGS_INDEX"
        value = "mya_nhsuk_${var.environment}"
      }
      env {
        name = "SPLUNK_TRACES_INDEX"
        value = "mya_nhsuk_${var.environment}"
      }
      env {
        name = "SPLUNK_METICS_INDEX"
        value = "mya_nhsuk_${var.environment}"
      }
      env {
        name = "SPLUNK_DATA_CHANNEL"
        value = var.splunk_data_channel
      }
      env {
        name = "SPLUNK_SKIP_VERIFY_INSECURE"
        value = var.splunk_skip_verify_insecure
      }
    }
  }

  ingress {
    external_enabled = true
    target_port = 4318
    traffic_weight {
      percentage = 100
      latest_revision = true
    }
  }
}

resource "azurerm_container_app_job" "nbs_mya_booking_extracts_job" {
  count                        = var.create_data_extracts ? 1 : 0 
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
  secret {
    name  = "container-cosmos-password"
    value = var.cosmos_token != "" ? var.cosmos_token : azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].primary_key
  }
  secret {
    name  = "blob-connection-string"
    value = var.data_extract_file_sender_options_type == "blob" ? azurerm_storage_account.nbs_mya_container_app_storage_account[0].primary_blob_connection_string : "UNSET"
  }
  secret {
    name  = "key-vault-secret"
    value = var.keyvault_client_secret
  }

  secret {
    name  = "app-config-connection-string"
    value = azurerm_app_configuration.nbs_mya_app_configuration[0].primary_read_key[0].connection_string
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
      env {
        name = "APP_CONFIG_CONNECTION"
        secret_name = "app-config-connection-string"
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
  secret {
    name  = "container-cosmos-password"
    value = var.cosmos_token != "" ? var.cosmos_token : azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].primary_key
  }
  
  secret {
    name  = "blob-connection-string"
    value = var.data_extract_file_sender_options_type == "blob" ? azurerm_storage_account.nbs_mya_container_app_storage_account[0].primary_blob_connection_string : "UNSET"
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

resource "azurerm_container_app" "nbs_mya_auditor" {
  count                        = var.auditor_enable ? 1 : 0
  name                         = "${var.application}-auditor-${var.environment}-${var.loc}"
  container_app_environment_id  = azurerm_container_app_environment.nbs_mya_container_enviroment.id
  resource_group_name           = local.resource_group_name
  revision_mode                 = "Single"

  secret {
    name  = "container-registry-password"
    value = var.container_registry_password
  }

  secret {
    name  = "container-cosmos-token"
    value = var.cosmos_token != "" ? var.cosmos_token : azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].primary_key
  }

  secret {
    name  = "auditor-blob-connection-string"
    value = azurerm_storage_account.nbs_mya_audit_storage_account.primary_connection_string
  }

  secret {
    name  = "splunk-hec-token"
    value = var.splunk_hec_token
  }

  registry {
    server             = var.container_registry_server_url
    username           = var.container_registry_username
    password_secret_name = "container-registry-password"
  }

  template {
    container {
      name   = "nbs-mya-auditor"
      image  = "${var.container_registry_server_url}/auditor:${var.build_number}"
      cpu    = 0.5 
      memory = "1Gi"

      env {
        name        = "Application_Name"
        value       = "${var.application}-auditor-${var.environment}-${var.loc}"
      }

      env {
        name        = "COSMOS_TOKEN"
        secret_name = "container-cosmos-token"
      }

      env {
        name        = "BLOB_STORAGE_CONNECTION_STRING"
        secret_name = "auditor-blob-connection-string"
      }

      env {
        name        = "SPLUNK_HEC_TOKEN"
        secret_name = "splunk-hec-token"
      }

      env {
        name  = "COSMOS_ENDPOINT"
        value = var.cosmos_endpoint != "" ? var.cosmos_endpoint : azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].endpoint
      }

      env {
        name  = "SPLUNK_HOST_URL"
        value = var.splunk_host_url
      }

      env {
        name  = "AuditWorkerConfigurations__0__PollingIntervalSeconds"
        value = 60
      }

      env {
        name  = "AuditWorkerConfigurations__0__ContainerName"
        value = var.auditor_worker_containers[0]
      }

      env {
        name  = "AuditWorkerConfigurations__0__LeaseContainerName"
        value = var.auditor_lease_container_name
      }

      env {
        name  = "AuditWorkerConfigurations__1__ContainerName"
        value = var.auditor_worker_containers[1]
      }

      env {
        name  = "AuditWorkerConfigurations__1__PollingIntervalSeconds"
        value = 60
      }

      env {
        name  = "AuditWorkerConfigurations__1__LeaseContainerName"
        value = var.auditor_lease_container_name
      }

      env {
        name  = "AuditWorkerConfigurations__2__ContainerName"
        value = var.auditor_worker_containers[2]
      }

      env {
        name  = "AuditWorkerConfigurations__2__PollingIntervalSeconds"
        value = 60
      }

      env {
        name  = "AuditWorkerConfigurations__2__LeaseContainerName"
        value = var.auditor_lease_container_name
      }

      env {
        name  = "AuditWorkerConfigurations__3__ContainerName"
        value = var.auditor_worker_containers[3]
      }

      env {
        name  = "AuditWorkerConfigurations__3__PollingIntervalSeconds"
        value = 60
      }

      env {
        name  = "AuditWorkerConfigurations__3__LeaseContainerName"
        value = var.auditor_lease_container_name
      }

      env {
        name  = "SinkExclusions__0__Source"
        value = var.auditor_sink_exclusions[0].source
      }

      env {
        name  = "SinkExclusions__0__ExcludedPaths__0"
        value = var.auditor_sink_exclusions[0].excluded_paths[0]
      }

      env {
        name  = "ChangeFeedPollingIntervalSeconds"
        value = var.aggregator_polling_interval_seconds
      }
    }
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_container_app" "nbs_mya_auditor" {
  count                        = var.auditor_enable ? 1 : 0
  name                         = "${var.application}-auditor-${var.environment}-${var.loc}"
  container_app_environment_id  = azurerm_container_app_environment.nbs_mya_container_enviroment.id
  resource_group_name           = local.resource_group_name
  revision_mode                 = "Single"

  secret {
    name  = "container-registry-password"
    value = var.container_registry_password
  }

  secret {
    name  = "container-cosmos-token"
    value = var.cosmos_token != "" ? var.cosmos_token : azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].primary_key
  }

  secret {
    name  = "splunk-hec-token"
    value = var.splunk_hec_token
  }

  registry {
    server             = var.container_registry_server_url
    username           = var.container_registry_username
    password_secret_name = "container-registry-password"
  }

  template {
    container {
      name   = "nbs-mya-aggregator"
      image  = "${var.container_registry_server_url}/aggregator:${var.build_number}"
      cpu    = 0.5 
      memory = "1Gi"

      env {
        name        = "Application_Name"
        value       = "${var.application}-aggregator-${var.environment}-${var.loc}"
      }

      env {
        name        = "COSMOS_TOKEN"
        secret_name = "container-cosmos-token"
      }

      env {
        name        = "SPLUNK_HEC_TOKEN"
        secret_name = "splunk-hec-token"
      }

      env {
        name  = "COSMOS_ENDPOINT"
        value = var.cosmos_endpoint != "" ? var.cosmos_endpoint : azurerm_cosmosdb_account.nbs_mya_cosmos_db[0].endpoint
      }

      env {
        name  = "SPLUNK_HOST_URL"
        value = var.splunk_host_url
      }

      env {
        name  = "CosmosTransactionOptions__MaxRetry"
        value = var.splunk_host_url
      }

      env {
        name  = "CosmosTransactionOptions__DefaultWaitSeconds"
        value = var.aggregator_default_wait_seconds
      }

      env {
        name  = "ChangeFeedPollingIntervalSeconds"
        value = var.aggregator_polling_interval_seconds
      }
    }
  }

  identity {
    type = "SystemAssigned"
  }
}