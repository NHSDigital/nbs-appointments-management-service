Feature: User Bulk Import

  Scenario: Import Users
    Given the default site for bulk import exists
    When I import the following users to the default site for bulk import
      | User               | FirstName | LastName | appointment-manager | availability-manager | site-details-manager | user-manager  | Region | ICB |
      | john.smith@nhs.net | John      | Smith    | FALSE               | TRUE                 | TRUE                 | TRUE          |        |     |
      | jane.smith@nhs.net | Jane      | Smith    | FALSE               | TRUE                 | TRUE                 | TRUE          |        |     |
    Then I receive a report that the import was successful
