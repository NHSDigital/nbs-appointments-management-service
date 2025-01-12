Feature: Authentication

  Scenario: Can fire an authenticate request
    Given the site is configured for MYA
    When I fire an authenticate request
    Then the call should be successful
