Feature: Get Roles

  Scenario: Retrieve list of roles
    Given There are existing roles
    When I query for all 'system' roles
    Then The following roles are returned
        | DisplayName                    | Id                           | Description                    |
        | Integration Test Api User Role | system:integration-test-user | Role for integration test user. |