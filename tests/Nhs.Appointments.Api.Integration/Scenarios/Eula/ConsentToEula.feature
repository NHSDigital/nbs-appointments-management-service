Feature: Eula

  Scenario: Consent to the latest EULA
    Given the latest EULA is as follows
      | VersionDate |
      | Today       |
    And I register and use a http client with details
      | User Id       | Eula Accepted Date  |
      | eulatestuser  | Yesterday           |
    When the api user agrees to a EULA with the following date
      | VersionDate |
      | Today       |
    Then the api user now has the following latest agreed EULA date
      | VersionDate |
      | Today       |
    And the 'core_data' document with id 'eula' and partition 'system' has lastUpdatedBy 'api@eulatestuser'
