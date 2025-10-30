Feature: Site Status Bulk Import

  Scenario: Toggle site soft deletion status
    Given a new site is configured for MYA
    When I bulk update the soft deletion status of the following sites
      | Name      |
      | Test Site |
    Then I receive a report that the import was successful
