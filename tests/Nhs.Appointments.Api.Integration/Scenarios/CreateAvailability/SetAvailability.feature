Feature: Set availability

  Scenario: Make a booking appointment
    Given the site is configured for MYA
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services |
      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID    |
    Then the request is successful and the following availability is created
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 1        |
    And the following availability created events are created
      | Type              | By       | FromDate | ToDate | Template_Days | FromTime | UntilTime | SlotLength | Capacity | Services |
      | SingleDateSession | api@test | Tomorrow |        |               | 09:00    | 17:00     | 5          | 1        | COVID    |