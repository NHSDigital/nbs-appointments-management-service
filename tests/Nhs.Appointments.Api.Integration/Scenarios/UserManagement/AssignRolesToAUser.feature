Feature: User Roles Assignment

  Scenario: Assign a role to a user
    Given There are no role assignments for user 'test-one'
    When I assign the following roles to user 'test-one'
      | Site | Roles                       |
      | A    | canned:site-details-manager |
    Then user 'test-one' would have the following role assignments
      | Site | Roles                       |
      | A    | canned:site-details-manager |

  Scenario: Assign a role to a user for a different site
    Given The following role assignments for 'test-two' exist
      | Site | Roles                       |
      | A    | canned:site-details-manager |
    When I assign the following roles to user 'test-two'
      | Site | Roles                       |
      | B    | canned:availability-manager |
    Then user 'test-two' would have the following role assignments
      | Site | Roles                       |
      | A    | canned:site-details-manager |
      | B    | canned:availability-manager |

  Scenario: Change existing role assignments at a site
    Given The following role assignments for 'test-three' exist
      | Site | Roles                       |
      | A    | canned:appointment-manager  |
      | A    | canned:site-details-manager |
    When I assign the following roles to user 'test-three'
      | Site | Roles                                                  |
      | A    | canned:appointment-manager,canned:availability-manager |
    Then user 'test-three' would have the following role assignments
      | Scope | Roles                       |
      | A     | canned:appointment-manager  |
      | A     | canned:availability-manager |
