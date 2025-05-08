Feature: Get Clinical Services

  Scenario: Get Clinical Services while Feature disabled
    Given feature toggle 'MultipleServices' is 'False'
    When I request Clinical Services
    Then the request should be Not Implemented
    And feature toggles are cleared

  Scenario: Get Clinical Services while Feature enabled
    Given feature toggle 'MultipleServices' is 'True'
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
    And feature toggles are cleared
