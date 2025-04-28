Feature: Proposing a potential new user

  Scenario: Propose a valid user
    Given There are no role assignments for user 'test-one'
    When I propose creating user 'test-one'
    Then current status of 'test-one' is returned as follows
      | ExtantInMya | ExtantInIdentityProvider | IdentityProvider | MeetsWhitelistRequirements |
      | False       | True                     | NhsMail          | True                       |
