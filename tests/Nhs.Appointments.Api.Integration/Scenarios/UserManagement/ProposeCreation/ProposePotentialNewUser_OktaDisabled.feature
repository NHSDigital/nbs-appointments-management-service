Feature: Proposing a potential new user - Okta Disabled
  
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
    
  Scenario: Propose an Okta user
    Given user 'test.user@my-pharmacy.co.uk' does not exist in MYA
    When I propose creating user 'test.user@my-pharmacy.co.uk'
    Then the request should be successful
    Then the user's current status is returned as follows
      | ExtantInSite | ExtantInIdentityProvider | IdentityProvider | MeetsWhitelistRequirements |
      | False       | False                    | Okta             | False                      |
