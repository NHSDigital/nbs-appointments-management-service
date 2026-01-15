Feature: Get Roles

  Scenario: Retrieve list of roles
    Given There are existing roles
    When I query for all 'system' roles
    Then The following roles are returned
        | DisplayName                    | Id                           | Description                                      |
        | Admin User                     | system:admin-user            | Admin user can view all areas that exist in MYA. |
        | Api User                       | system:api-user              | This is a dedicated NBS role                     |
        | All Permissions                | system:all-permissions       | System role used for full api access for development and test purposes (ONLY FOR USE IN TEST ENVIRONMENTS).  |
        | Integration Test Api User Role | system:integration-test-user | Role for integration test user.                  |
        | Regional User                  | system:regional-user         | System role to give a user permissions to all sites within a region                 |
        | ICB User                       | system:icb-user              | System role to give a user permissions to all sites within an ICB         |
