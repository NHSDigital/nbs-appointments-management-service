Feature: Audit Trail Synchronization - Notifications

  Scenario: Update user roles, notification raised and file appears in Blob
     Given user 'test-new-audit-user' does not exist in the system
     When I assign the following roles to user 'test-new-audit-user'
      | Site | Roles                       |
      | A    | canned:site-details-manager |
     Then UserRolesChanged notification should be audited in StorageAccount
  
    
