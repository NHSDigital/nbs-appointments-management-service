resource "azurerm_cdn_frontdoor_profile" "nbs_mya_frontdoor_profile" {
  count                    = var.create_frontdoor ? 1 : 0
  name                     = "${var.application}-fdtest-${var.environment}-${var.loc}"
  resource_group_name      = data.azurerm_resource_group.nbs_mya_resource_group.name
  sku_name                 = "Standard_AzureFrontDoor"
  response_timeout_seconds = 60
}

resource "azurerm_cdn_frontdoor_endpoint" "nbs_mya_endpoint" {
  count                    = var.create_frontdoor ? 1 : 0
  name                     = "nbs-mya"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.nbs_mya_frontdoor_profile[0].id
}

# Api
resource "azurerm_cdn_frontdoor_origin_group" "nbs_mya_api_origin_group" {
  count                    = var.create_frontdoor ? 1 : 0
  name                     = "mya-api-test"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.nbs_mya_frontdoor_profile[0].id
  session_affinity_enabled = false

  restore_traffic_time_to_healed_or_new_endpoint_in_minutes = 10

  load_balancing {
    additional_latency_in_milliseconds = 50
    sample_size                        = 4
    successful_samples_required        = 3
  }
}

resource "azurerm_cdn_frontdoor_origin" "nbs_mya_api_origin" {
  count                         = var.create_frontdoor ? 1 : 0
  name                          = "mya-api-test"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.nbs_mya_api_origin_group[0].id
  enabled                       = true

  certificate_name_check_enabled = true

  host_name          = azurerm_windows_function_app.nbs_mya_func_app.default_hostname
  http_port          = 80
  https_port         = 443
  origin_host_header = azurerm_windows_function_app.nbs_mya_func_app.default_hostname
  priority           = 1
  weight             = 1000
}

resource "azurerm_cdn_frontdoor_route" "nbs_mya_api_route" {
  count                         = var.create_frontdoor ? 1 : 0
  name                          = "apitest-route"
  cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.nbs_mya_endpoint[0].id
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.nbs_mya_api_origin_group[0].id
  cdn_frontdoor_origin_ids      = [azurerm_cdn_frontdoor_origin.nbs_mya_api_origin[0].id]
  enabled                       = true
  cdn_frontdoor_origin_path     = "/api"

  forwarding_protocol    = "MatchRequest"
  https_redirect_enabled = false
  patterns_to_match      = ["/manage-your-appointments/api/*"]
  supported_protocols    = ["Https"]

  link_to_default_domain = true
}

# Web app
resource "azurerm_cdn_frontdoor_origin_group" "nbs_mya_web_origin_group" {
  count                    = var.create_frontdoor ? 1 : 0
  name                     = "mya-web-test"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.nbs_mya_frontdoor_profile[0].id
  session_affinity_enabled = false

  restore_traffic_time_to_healed_or_new_endpoint_in_minutes = 10

  load_balancing {
    additional_latency_in_milliseconds = 50
    sample_size                        = 4
    successful_samples_required        = 3
  }
}

resource "azurerm_cdn_frontdoor_origin" "nbs_mya_web_origin" {
  count                         = var.create_frontdoor ? 1 : 0
  name                          = "mya-web-test"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.nbs_mya_web_origin_group[0].id
  enabled                       = true

  certificate_name_check_enabled = true

  host_name          = azurerm_linux_web_app.nbs_mya_web_app_service.default_hostname
  http_port          = 80
  https_port         = 443
  origin_host_header = azurerm_linux_web_app.nbs_mya_web_app_service.default_hostname
  priority           = 1
  weight             = 1000
}

resource "azurerm_cdn_frontdoor_route" "nbs_mya_web_route" {
  count                         = var.create_frontdoor ? 1 : 0
  name                          = "webtest-route"
  cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.nbs_mya_endpoint[0].id
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.nbs_mya_web_origin_group[0].id
  cdn_frontdoor_origin_ids      = [azurerm_cdn_frontdoor_origin.nbs_mya_web_origin[0].id]
  enabled                       = true

  forwarding_protocol    = "MatchRequest"
  https_redirect_enabled = false
  patterns_to_match      = ["/manage-your-appointments/*"]
  supported_protocols    = ["Https"]

  link_to_default_domain = true
}
