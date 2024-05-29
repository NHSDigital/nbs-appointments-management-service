output "module_web_storage_name" {
  value = azurerm_storage_account.nbs_appts_web_stacc.name
}

output "module_func_app_url" {
  value = azurerm_windows_function_app.nbs_appts_func_app.default_hostname
}