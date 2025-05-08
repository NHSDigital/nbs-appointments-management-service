Feature: Proposing a potential new user

  Scenario: Propose a valid NHS user
    Given feature toggle 'OktaEnabled' is 'False'
    And user 'test.user@nhs.net' does not exist in MYA
    When I propose creating user 'test.user@nhs.net'
    Then the request should be successful
    Then the user's current status is returned as follows
      | ExtantInSite | ExtantInIdentityProvider | IdentityProvider | MeetsWhitelistRequirements |
      | False       | True                     | NhsMail          | True                       |
    And feature toggles are cleared

  Scenario: Propose a valid NHS user who already exists in MYA
    Given feature toggle 'OktaEnabled' is 'False'
    And user 'test.user@nhs.net' exists in MYA
    When I propose creating user 'test.user@nhs.net'
    Then the request should be successful
    Then the user's current status is returned as follows
      | ExtantInSite | ExtantInIdentityProvider | IdentityProvider | MeetsWhitelistRequirements |
      | True        | True                     | NhsMail          | True                       |
    And feature toggles are cleared

  Scenario: Propose a valid Okta user
    Given feature toggle 'OktaEnabled' is 'True'
    Given user 'test.user@my-pharmacy.co.uk' does not exist in MYA
    When I propose creating user 'test.user@boots.com'
    Then the request should be successful
    Then the user's current status is returned as follows
      | ExtantInSite | ExtantInIdentityProvider | IdentityProvider | MeetsWhitelistRequirements |
      | False       | False                    | Okta             | True                       |
    And feature toggles are cleared

  Scenario: Propose an invalid Okta user
    Given feature toggle 'OktaEnabled' is 'True'
    Given user 'test.user@not-in-domains-whitelist.co.uk' does not exist in MYA
    When I propose creating user 'test.user@not-in-domains-whitelist.co.uk'
    Then the request should be successful
    Then the user's current status is returned as follows
      | ExtantInSite | ExtantInIdentityProvider | IdentityProvider | MeetsWhitelistRequirements |
      | False       | False                    | Okta             | False                      |
    And feature toggles are cleared

  Scenario: Propose an Okta user with Okta toggled off
    Given feature toggle 'OktaEnabled' is 'False'
    Given user 'test.user@my-pharmacy.co.uk' does not exist in MYA
    When I propose creating user 'test.user@my-pharmacy.co.uk'
    Then the request should be successful
    Then the user's current status is returned as follows
      | ExtantInSite | ExtantInIdentityProvider | IdentityProvider | MeetsWhitelistRequirements |
      | False       | False                    | Okta             | False                      |
    And feature toggles are cleared
