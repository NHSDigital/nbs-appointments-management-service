Feature: Site Status Bulk Import

  Scenario: Toggle site soft deletion status
    Given the default site for bulk import exists
    When I bulk update the soft deletion status of the following sites
      | Name      |
      | Test Site |
    Then I receive a report that the import was successful
