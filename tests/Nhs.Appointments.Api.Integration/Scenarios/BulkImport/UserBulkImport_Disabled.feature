Feature: User Bulk Import

  Scenario: Import Users
    Given a new site is configured for MYA
    When I import the following users
      | User               | FirstName | LastName | appointment-manager | availability-manager | site-details-manager | user-manager  | Region |
      | john.smith@nhs.net | John      | Smith    | FALSE               | TRUE                | TRUE                  | FALSE         |        |
      | jane.smith@nhs.net | Jane      | Smith    | FALSE               | TRUE                | TRUE                  | FALSE         |        |
    Then the call should fail with 501
