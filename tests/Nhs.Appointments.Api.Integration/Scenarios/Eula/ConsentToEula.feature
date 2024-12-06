Feature: Eula

  Scenario: Consent to the latest EULA
    Given the latest EULA is as follows
      | VersionDate |
      | Today       |
    And the current user has agreed the EULA on the following date
      | VersionDate |
      | Yesterday   |
    When the current user agrees to a EULA with the following date
      | VersionDate |
      | Today   |
    Then the current user now has the following latest agreed EULA date
      | VersionDate |
      | Today       |