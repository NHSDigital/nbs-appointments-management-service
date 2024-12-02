Feature: Eula

  Scenario: Can get the latest EULA version
    Given the latest EULA is as follows
      | VersionDate |
      | Today       |
    And I request the latest EULA version
    Then the following EULA is returned
      | VersionDate |
      | Today       |