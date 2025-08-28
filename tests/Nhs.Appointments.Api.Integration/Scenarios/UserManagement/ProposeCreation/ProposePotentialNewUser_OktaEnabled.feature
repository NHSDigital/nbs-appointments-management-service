Feature: Proposing a potential new user - Okta Enabled

  Scenario: Propose a valid NHS user
    Given user 'test.user@nhs.net' does not exist in MYA
    When I propose creating user 'test.user@nhs.net'
    Then the request should be successful
    Then the user's current status is returned as follows
      | ExtantInSite | ExtantInIdentityProvider | IdentityProvider | MeetsWhitelistRequirements |
      | False       | True                     | NhsMail          | True                       |

  Scenario: Propose a valid NHS user who already exists in MYA
    Given user 'test.user@nhs.net' exists in MYA
    When I propose creating user 'test.user@nhs.net'
    Then the request should be successful
    Then the user's current status is returned as follows
      | ExtantInSite | ExtantInIdentityProvider | IdentityProvider | MeetsWhitelistRequirements |
      | True        | True                     | NhsMail          | True                       |
  
  Scenario: Propose a valid Okta user
    Given user 'test.user@my-pharmacy.co.uk' does not exist in MYA
    When I propose creating user 'test.user@boots.com'
    Then the request should be successful
    Then the user's current status is returned as follows
      | ExtantInSite | ExtantInIdentityProvider | IdentityProvider | MeetsWhitelistRequirements |
      | False       | False                    | Okta             | True                       |

  Scenario: Propose an invalid Okta user
    Given user 'test.user@not-in-domains-whitelist.co.uk' does not exist in MYA
    When I propose creating user 'test.user@not-in-domains-whitelist.co.uk'
    Then the request should be successful
    Then the user's current status is returned as follows
      | ExtantInSite | ExtantInIdentityProvider | IdentityProvider | MeetsWhitelistRequirements |
      | False       | False                    | Okta             | False                      |
