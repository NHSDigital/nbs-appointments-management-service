resource "azurerm_cdn_frontdoor_profile" "nbs_mya_frontdoor_profile" {
  count                    = var.create_frontdoor ? 1 : 0
  name                     = "${var.application}-fd-${var.environment}-${var.loc}"
  resource_group_name      = data.azurerm_resource_group.nbs_mya_resource_group.name
  sku_name                 = "Standard_AzureFrontDoor"
  response_timeout_seconds = 60
}

resource "azurerm_cdn_frontdoor_endpoint" "nbs_mya_endpoint" {
  count                    = var.create_frontdoor ? 1 : 0
  name                     = "nbs-mya-${var.environment}"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.nbs_mya_frontdoor_profile[0].id
}

# Http function app origin group and route
resource "azurerm_cdn_frontdoor_origin_group" "nbs_mya_http_function_app_origin_group" {
  count                    = var.create_frontdoor ? 1 : 0
  name                     = "mya-http-api-${var.environment}"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.nbs_mya_frontdoor_profile[0].id
  session_affinity_enabled = false

  restore_traffic_time_to_healed_or_new_endpoint_in_minutes = 10

  load_balancing {
    additional_latency_in_milliseconds = 50
    sample_size                        = 4
    successful_samples_required        = 3
  }
}

resource "azurerm_cdn_frontdoor_origin" "nbs_mya_http_function_app_origin" {
  count                         = var.create_frontdoor ? 1 : 0
  name                          = "mya-http-api-${var.environment}"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.nbs_mya_http_function_app_origin_group[0].id
  enabled                       = true

  certificate_name_check_enabled = true

  host_name          = azurerm_windows_function_app.nbs_mya_http_func_app.default_hostname
  http_port          = 80
  https_port         = 443
  origin_host_header = azurerm_windows_function_app.nbs_mya_http_func_app.default_hostname
  priority           = 1
  weight             = 1000
}

resource "azurerm_cdn_frontdoor_route" "nbs_mya_http_function_route" {
  count                         = var.create_frontdoor ? 1 : 0
  name                          = "http-api-route-${var.environment}"
  cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.nbs_mya_endpoint[0].id
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.nbs_mya_http_function_app_origin_group[0].id
  cdn_frontdoor_origin_ids      = [azurerm_cdn_frontdoor_origin.nbs_mya_http_function_app_origin[0].id]
  cdn_frontdoor_rule_set_ids    = [azurerm_cdn_frontdoor_rule_set.nbs_mya_origin_group_override_rule_set[0].id]
  enabled                       = true
  cdn_frontdoor_origin_path     = "/api"

  forwarding_protocol    = "MatchRequest"
  https_redirect_enabled = false
  patterns_to_match      = ["/manage-your-appointments/api/*"]
  supported_protocols    = ["Https"]

  link_to_default_domain = true
}

# High load function app origin group
resource "azurerm_cdn_frontdoor_origin_group" "nbs_mya_high_load_function_app_origin_group" {
  count                    = var.create_frontdoor ? 1 : 0
  name                     = "mya-high-load-api-${var.environment}"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.nbs_mya_frontdoor_profile[0].id
  session_affinity_enabled = false

  restore_traffic_time_to_healed_or_new_endpoint_in_minutes = 10

  load_balancing {
    additional_latency_in_milliseconds = 50
    sample_size                        = 4
    successful_samples_required        = 3
  }
}

resource "azurerm_cdn_frontdoor_origin" "nbs_mya_high_load_function_app_origin" {
  count                         = var.create_frontdoor ? 1 : 0
  name                          = "mya-high-load-api-${var.environment}"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.nbs_mya_high_load_function_app_origin_group[0].id
  enabled                       = true

  certificate_name_check_enabled = true

  host_name          = azurerm_windows_function_app.nbs_mya_high_load_func_app[0].default_hostname
  http_port          = 80
  https_port         = 443
  origin_host_header = azurerm_windows_function_app.nbs_mya_high_load_func_app[0].default_hostname
  priority           = 1
  weight             = 1000
}

# Origin group override rule set - directs any calls to availability/query endpoint to high load function app where this function is running
resource "azurerm_cdn_frontdoor_rule_set" "nbs_mya_origin_group_override_rule_set" {
  count                    = var.create_frontdoor ? 1 : 0
  name                     = "MyaOverrideRuleSet"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.nbs_mya_frontdoor_profile[0].id
}

resource "azurerm_cdn_frontdoor_rule" "nbs_mya_origin_group_override_rule" {
  count = var.create_frontdoor ? 1 : 0
  depends_on = [
    azurerm_cdn_frontdoor_origin_group.nbs_mya_http_function_app_origin_group,
    azurerm_cdn_frontdoor_origin.nbs_mya_http_function_app_origin,
    azurerm_cdn_frontdoor_origin_group.nbs_mya_high_load_function_app_origin_group,
    azurerm_cdn_frontdoor_origin.nbs_mya_high_load_function_app_origin
  ]

  name                      = "AvailabilityQueryRouteOverride"
  cdn_frontdoor_rule_set_id = azurerm_cdn_frontdoor_rule_set.nbs_mya_origin_group_override_rule_set[0].id
  order                     = 1
  behavior_on_match         = "Continue"

  actions {
    route_configuration_override_action {
      cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.nbs_mya_high_load_function_app_origin_group[0].id
      forwarding_protocol           = "MatchRequest"
      compression_enabled           = false
      cache_behavior                = "Disabled"
    }
  }

  conditions {
    url_path_condition {
      operator         = "EndsWith"
      negate_condition = false
      match_values     = ["/availability/query"]
      transforms       = ["Lowercase"]
    }
  }
}

# Web app
resource "azurerm_cdn_frontdoor_origin_group" "nbs_mya_web_origin_group" {
  count                    = var.create_frontdoor ? 1 : 0
  name                     = "mya-web"
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
  name                          = "mya-web"
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
  name                          = "web-route"
  cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.nbs_mya_endpoint[0].id
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.nbs_mya_web_origin_group[0].id
  cdn_frontdoor_origin_ids      = [azurerm_cdn_frontdoor_origin.nbs_mya_web_origin[0].id]
  enabled                       = true

  forwarding_protocol    = "MatchRequest"
  https_redirect_enabled = false
  patterns_to_match      = ["/manage-your-appointments/*", "/manage-your-appointments"]
  supported_protocols    = ["Https"]

  link_to_default_domain = true
}
