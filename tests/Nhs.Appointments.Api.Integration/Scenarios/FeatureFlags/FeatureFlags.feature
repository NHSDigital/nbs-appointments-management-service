Feature: Feature Flags

  Scenario: Local flag TestFeatureEnabled is enabled
    When I request the enabled state for feature flag 'TestFeatureEnabled'
    Then the response should be 200 with enabled state 'true'

  Scenario: Local flag TestFeatureDisabled is disabled
    When I request the enabled state for feature flag 'TestFeatureDisabled'
    Then the response should be 200 with enabled state 'false'

  Scenario: Local flag TestFeatureUsersEnabled is enabled for users within the targeting audience
    When I request the user and site enabled state for feature flag 'TestFeatureUsersEnabled' with user 'api@test' and site ''
    Then the response should be 200 with enabled state 'true'

  Scenario: Local flag TestFeatureUsersEnabled is disabled for users outside the targeting audience
    When I request the user and site enabled state for feature flag 'TestFeatureUsersEnabled' with user 'user1@nhs.net' and site ''
    Then the response should be 200 with enabled state 'false'

  Scenario: Local flag TestFeatureSitesEnabled is enabled for sites within the targeting audience
    When I request the user and site enabled state for feature flag 'TestFeatureSitesEnabled' with user '' and site '24c36a82-489e-4fc1-877a-6e8cae0deaae'
    Then the response should be 200 with enabled state 'true'

  Scenario: Local flag TestFeatureSitesEnabled is disabled for sites outside the targeting audience
    When I request the user and site enabled state for feature flag 'TestFeatureSitesEnabled' with user '' and site '5bb9177c-1555-42a5-91cd-a2c3c8efa0ff'
    Then the response should be 200 with enabled state 'false'

# Logical OR
  Scenario: Local flag TestFeatureSiteOrUserEnabled is enabled for users within the targeting audience but the site is outside the targeting audience
    When I request the user and site enabled state for feature flag 'TestFeatureSiteOrUserEnabled' with user 'api@test' and site '24c36a82-489e-4fc1-877a-6e8cae0deaae'
    Then the response should be 200 with enabled state 'true'

# Logical OR
  Scenario: Local flag TestFeatureSiteOrUserEnabled is enabled for users outside the targeting audience but the site is within the targeting audience
    When I request the user and site enabled state for feature flag 'TestFeatureSiteOrUserEnabled' with user 'test@nhs.net' and site '5bb9177c-1555-42a5-91cd-a2c3c8efa0ff'
    Then the response should be 200 with enabled state 'true'

# Logical OR (AND case)
  Scenario: Local flag TestFeatureSiteOrUserEnabled is enabled for users within the targeting audience and the site is within the targeting audience
    When I request the user and site enabled state for feature flag 'TestFeatureSiteOrUserEnabled' with user 'api@test' and site '5bb9177c-1555-42a5-91cd-a2c3c8efa0ff'
    Then the response should be 200 with enabled state 'true'
    
# Logical OR (Neither case)
  Scenario: Local flag TestFeatureSiteOrUserEnabled is disabled when the user is outside the targeting audience and the site is outside the targeting audience
    When I request the user and site enabled state for feature flag 'TestFeatureSiteOrUserEnabled' with user 'test@nhs.net' and site '24c36a82-489e-4fc1-877a-6e8cae0deaae'
    Then the response should be 200 with enabled state 'false'
