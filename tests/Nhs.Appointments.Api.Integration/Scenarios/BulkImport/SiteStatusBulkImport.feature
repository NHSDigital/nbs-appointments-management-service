Feature: Site Status Bulk Import

  Scenario: Import Users
    Given a new site is configured for MYA
    When I bulk update the soft deletion status of the following sites
      | Name      |
      | Test Site |
    Then I receive a report that the import was successful
