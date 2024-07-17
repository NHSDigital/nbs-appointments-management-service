Feature: User Roles Assignment

  Scenario: Assign a role to a user
    Given There are no role assignments for user 'test-one'
    When I assign the following roles to user 'test-one'
        | Scope     | Roles                             |
        | site:1000 | canned:site-configuration-manager |
    Then user 'test-one' would have the following role assignments
        | Scope     | Roles                             |
        | site:1000 | canned:site-configuration-manager |

  Scenario: Assign a role to a user for a different site
    Given The following role assignments for 'test-two' exist 
      | Scope     | Roles                             |
      | site:1000 | canned:site-configuration-manager |
    When I assign the following roles to user 'test-two'
      | Scope     | Roles                       |
      | site:2000 | canned:availability-manager |
    Then user 'test-two' would have the following role assignments
      | Scope     | Roles                             |
      | site:1000 | canned:site-configuration-manager |
      | site:2000 | canned:availability-manager       |

  Scenario: Change existing role assignments at a site
    Given The following role assignments for 'test-three' exist
      | Scope     | Roles                             |
      | site:1000 | canned:appointment-manager        |
      | site:1000 | canned:site-configuration-manager |
    When I assign the following roles to user 'test-three'
      | Scope     | Roles                                      |
      | site:1000 | canned:appointment-manager,canned:check-in |
    Then user 'test-three' would have the following role assignments
      | Scope     | Roles                      |
      | site:1000 | canned:appointment-manager |
      | site:1000 | canned:check-in            |