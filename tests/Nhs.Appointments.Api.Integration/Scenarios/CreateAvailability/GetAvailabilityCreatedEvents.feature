Feature: Create daily availability

  Scenario: Can apply an availability template
    Given there is no existing availability
    When I apply the following availability template
      | From     | Until       | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services |
      | Tomorrow | Tomorrow_+2 | Relative | 09:00    | 10:00     | 5          | 1        | COVID    |
    And I apply the following availability
      | Date        | From  | Until | SlotLength | Capacity | Services |
      | Tomorrow_+5 | 09:00 | 17:00 | 5          | 1        | COVID    |
    And the following availability created events are created
      | Type              | By       | FromDate    | ToDate      | Template_Days | FromTime | UntilTime | SlotLength | Capacity | Services |
      | Template          | api@test | Tomorrow    | Tomorrow_+2 | Relative      | 09:00    | 10:00     | 5          | 1        | COVID    |
      | SingleDateSession | api@test | Tomorrow_+5 |             |               | 09:00    | 17:00     | 5          | 1        | COVID    |
    Then I request Availability Created Events for the current site