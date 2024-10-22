Feature: Remove User roles from a Site

  Scenario: Remove a user from a site
    Given The following role assignments for 'test-one' exist
      | Site | Roles                             |
      | A    | canned:availability-manager       |
    When I remove user 'test-one' from site 'A'
    Then 'test-one' is no longer in the system

  Scenario: Remove a user from one site, leaving them at others
    Given The following role assignments for 'test-two' exist
      | Site | Roles                       |
      | A    | canned:availability-manager |
      | B    | canned:availability-manager |
      | C    | canned:check-in             |
    When I remove user 'test-two' from site 'A'
    Then user 'test-two' would have the following role assignments
      | Scope | Roles                      |
      | B     | canned:availability-manager |
      | C     | canned:check-in            |
