Feature: Blob Audit Audit Data - Notifications

  Scenario: Update user roles, notification raised and file appears in Blob
     Given user 'test-new-audit-user@nhs.net' does not exist in the system
     When I assign the following roles to user 'test-new-audit-user@nhs.net'
      | Site | Roles                       |
      | A    | canned:site-details-manager |
     Then UserRolesChanged audit notification should match the notification in Cosmos DB
  
    
