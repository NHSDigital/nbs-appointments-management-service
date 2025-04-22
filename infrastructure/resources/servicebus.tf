## Service Bus
resource "azurerm_servicebus_namespace" "nbs_mya_service_bus" {
  name                = "${var.application}-sb-${var.environment}-${var.loc}"
  location            = var.location
  resource_group_name = data.azurerm_resource_group.nbs_mya_resource_group.name
  sku                 = "Standard"
}

resource "azurerm_servicebus_queue" "nbs_mya_sbq_user_roles" {
  name         = "user-roles-changed"
  namespace_id = azurerm_servicebus_namespace.nbs_mya_service_bus.id
}

resource "azurerm_servicebus_queue" "nbs_mya_sbq_booking_made" {
  name         = "booking-made"
  namespace_id = azurerm_servicebus_namespace.nbs_mya_service_bus.id
}

resource "azurerm_servicebus_queue" "nbs_mya_sbq_booking_cancelled" {
  name         = "booking-cancelled"
  namespace_id = azurerm_servicebus_namespace.nbs_mya_service_bus.id
}

resource "azurerm_servicebus_queue" "nbs_mya_sbq_bookingreminder" {
  name         = "booking-reminder"
  namespace_id = azurerm_servicebus_namespace.nbs_mya_service_bus.id
}

resource "azurerm_servicebus_queue" "nbs_mya_sbq_booking_rescheduled" {
  name         = "booking-rescheduled"
  namespace_id = azurerm_servicebus_namespace.nbs_mya_service_bus.id
}
