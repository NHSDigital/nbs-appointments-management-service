
## Storage account and container for web app

resource "azurerm_storage_account" "nbs_appts_web_stacc" {
  name                     = "${var.application_short}web${var.environment}${var.loc}"
  resource_group_name      = data.azurerm_resource_group.nbs_appts_rg.name
  location                 = data.azurerm_resource_group.nbs_appts_rg.location
  account_replication_type = "LRS"
  account_tier             = "Standard"

  static_website {
    index_document     = "index.html"
    error_404_document = "index.html"
  }
}