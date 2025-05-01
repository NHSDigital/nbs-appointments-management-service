Feature: Proposing a potential new user

  Scenario: Propose a valid NHS user
    Given user 'test-one@nhs.net' does not exist in MYA
    When I propose creating user 'test-one@nhs.net'
    Then the request should be successful
    Then the user's current status is returned as follows
      | ExtantInMya | ExtantInIdentityProvider | IdentityProvider | MeetsWhitelistRequirements |
      | False       | True                     | NhsMail          | True                       |
