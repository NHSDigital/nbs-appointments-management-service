Feature: Get User Role Assignments For A Site

  Scenario: Get all user role assignments for a designated site
    Given The following role assignments for 'test-one' exist
      | Site | Roles                       |
      | A    | canned:site-details-manager |
    And the following role assignments for 'test-two' exist
      | Site | Roles                             |
      | A    | canned:availability-manager       |
      | B    | canned:site-details-manager |
    When I request all user role assignments for site 'A'
    Then the following list of user role assignments is returned
      | User     | Site | Roles                             |
      | test-one | A    | canned:site-details-manager |
      | test-two | A    | canned:availability-manager       |
    
