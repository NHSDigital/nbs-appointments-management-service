Feature: Create daily availability

  Scenario: Can apply an availability template
    Given the default site exists
    When I apply the following availability template to the default site
      | From     | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services | Mode      |
      | Tomorrow | 3 days from today | Relative | 09:00    | 10:00     | 5          | 1        | COVID    | Additive  |
    And I apply the following availability to the default site
      | Date              | From  | Until | SlotLength | Capacity | Services | Mode      |
      | 6 days from today | 09:00 | 17:00 | 5          | 1        | COVID    | Overwrite |
    Then the following availability created events are created at the default site
      | Type              | By       | FromDate          | ToDate            | Template_Days | FromTime | UntilTime | SlotLength | Capacity | Services |
      | Template          | api@test | Tomorrow          | 3 days from today | Relative      | 09:00    | 10:00     | 5          | 1        | COVID    |
      | SingleDateSession | api@test | 6 days from today |                   |               | 09:00    | 17:00     | 5          | 1        | COVID    |
    Then I request availability created events for the default site
