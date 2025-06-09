Feature: Get Clinical Services

  Scenario: Get Clinical Services
    When I request Clinical Services
    Then the request should be Not Implemented

  Scenario: Get Clinical Services even with data available
    Given I have Clinical Services
      | Service |
      | RSV     |
      | Covid   |
      | Flu     |
    When I request Clinical Services
    Then the request should be Not Implemented
