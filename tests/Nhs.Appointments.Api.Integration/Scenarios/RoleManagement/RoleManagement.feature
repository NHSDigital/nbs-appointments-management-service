﻿Feature: Get Roles

  Scenario: Retrieve list of roles
    Given There are existing roles
    When I query for all roles in the system
    Then The following roles are returned
        | DisplayName                    | Id                        | Description                    |
        | Integration Test Api User Role | integration-test:api-user | Role for integration test user |