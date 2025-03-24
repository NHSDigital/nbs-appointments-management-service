Feature: Feature Flags

  Scenario: Local flag TestFeatureEnabled is enabled
    When I request the enabled state for feature flag 'TestFeatureEnabled'
    Then the response should be 200 with enabled state 'true'

  Scenario: Local flag TestFeatureDisabled is disabled
    When I request the enabled state for feature flag 'TestFeatureDisabled'
    Then the response should be 200 with enabled state 'false'

  Scenario: Local flag TestFeatureTimeWindowDisabled is disabled
    When I request the enabled state for feature flag 'TestFeatureTimeWindowDisabled'
    Then the response should be 200 with enabled state 'false'

  Scenario: Local flag TestFeatureTimeWindowEnabled is enabled
    When I request the enabled state for feature flag 'TestFeatureTimeWindowEnabled'
    Then the response should be 200 with enabled state 'true'
