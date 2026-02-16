Feature: Blob Audit Core Data

  Scenario: Add user, file appears in Blob
    Given user 'test-new-audit-user@nhs.net' does not exist in the system
    When I assign the following roles to user 'test-new-audit-user@nhs.net'
      | Site | Roles                       |
      | A    | canned:site-details-manager |
    Then user 'test-new-audit-user@nhs.net' would have the following role assignments
      | Site | Roles                       |
      | A    | canned:site-details-manager |
    And an audit log should be created in StorageAccount for user 'test-new-audit-user@nhs.net'
    And the audit log for 'test-new-audit-user@nhs.net' should match the Cosmos DB record

  Scenario: Update user roles, file appears in Blob
    Given there are no role assignments for user 'test-auditor@nhs.net'
    When I assign the following roles to user 'test-auditor@nhs.net'
      | Site | Roles                       |
      | A    | canned:site-details-manager |
    Then user 'test-auditor@nhs.net' would have the following role assignments
      | Site | Roles                       |
      | A    | canned:site-details-manager |
    And an audit log should be created in StorageAccount for user 'test-auditor@nhs.net'
    And the audit log for 'test-auditor@nhs.net' should match the Cosmos DB record

    
  Scenario: Update reference details of a site, file appears in Blob
    Given the following sites exist in the system
      | Site                                 | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                       | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A New Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true  | -60       | -60      |
    When I update the reference details for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51'
      | OdsCode     | ICB  | Region |
      | 16B         | ICB2 | R34    |
    Then the correct information for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51' is returned
      | Site                                 | Name   | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                       | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A New Lane | 0113 1111111 | 16B     | R34    | ICB2 | Info 1                 | def_one/attr_one=true, def_one/attr_two=false, def_two/attr_one=true  | -60       | -60      |
    And an audit log should be created in StorageAccount for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51'
    And the audit log for site 'beeae4e0-dd4a-4e3a-8f4d-738f9418fb51' should match the Cosmos DB record
