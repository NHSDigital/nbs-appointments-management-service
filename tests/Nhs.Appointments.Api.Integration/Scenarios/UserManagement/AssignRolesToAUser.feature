Feature: User Roles Assignment

  Scenario: Assign a role to a user - audit trail
    Given there are no role assignments for user 'test-one'
    When I assign the following roles to user 'test-one'
      | Site                                   | Roles                       |
      | 562348bf-3509-45f2-887c-4f9651501f06   | canned:site-details-manager |
    Then user 'test-one' would have the following role assignments
      | Site                                   | Roles                       |
      | 562348bf-3509-45f2-887c-4f9651501f06   | canned:site-details-manager |
    And a 'test-one' user should be audited in blob storage
    And a 'UserRolesChanged' notification should be audited for user 'test-one' in blob storage

  Scenario: Assign a role to a user for a different site
    Given the following role assignments at site 'A' for 'test-two' exist
      | Roles                       |
      | canned:site-details-manager |
    When I assign the following roles to user 'test-two'
      | Site | Roles                       |
      | B    | canned:availability-manager |
    Then user 'test-two' would have the following role assignments
      | Site | Roles                       |
      | A    | canned:site-details-manager |
      | B    | canned:availability-manager |

  Scenario: Change existing role assignments at a site
    Given the following role assignments at site 'A' for 'test-three' exist
      | Roles                       |
      | canned:appointment-manager  |
      | canned:site-details-manager |
    When I assign the following roles to user 'test-three'
      | Site | Roles                                                  |
      | A    | canned:appointment-manager,canned:availability-manager |
    Then user 'test-three' would have the following role assignments
      | Scope | Roles                       |
      | A     | canned:appointment-manager  |
      | A     | canned:availability-manager |
