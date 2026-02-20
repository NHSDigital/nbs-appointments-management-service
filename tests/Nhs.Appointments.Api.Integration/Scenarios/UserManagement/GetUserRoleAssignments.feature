Feature: Get User Role Assignments For A Site

  Scenario: Get all user role assignments for a designated site
    Given the following role assignments at the default site for 'test-one' exist
      | Roles                       |
      | canned:site-details-manager |
    And the following role assignments at the default site for 'test-two' exist
      | Roles                             |
      | canned:availability-manager       |
      | canned:site-details-manager       |
    When I request all user role assignments at the default site
    Then the following list of user role assignments at the default site is returned
      | User     | Roles                                                      |
      | test-one | canned:site-details-manager                                |
      | test-two | canned:availability-manager, canned:site-details-manager   |
    
