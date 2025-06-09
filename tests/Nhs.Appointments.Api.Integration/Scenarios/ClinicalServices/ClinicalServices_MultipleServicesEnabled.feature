Feature: Get Clinical Services for MultipleServices Enabled

  Scenario: Get Clinical Services Single Configured
    And I have Clinical Services
      | Service |
      | RSV     |
    When I request Clinical Services
    Then the request should return Clinical Services
      | Service |
      | RSV     |
    Then the request should be successful
  
  Scenario: Get Clinical Services
    And I have Clinical Services
      | Service |
      | RSV     |
      | Covid   |
      | Flu     |
    When I request Clinical Services
    Then the request should return Clinical Services
      | Service |
      | RSV     |
      | Covid   |
      | Flu     |
    Then the request should be successful

  
