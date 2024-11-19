Feature: Create daily availability

  Scenario: Can apply an availability template
    Given there is no existing availability
    When I apply the following availability template
      | From     | Until       | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services |
      | Tomorrow | Tomorrow_+2 | Relative | 09:00    | 10:00     | 5          | 1        | COVID    |
    Then the request is successful and the following daily availability is created
      | Date        | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 10:00 | COVID    | 5           | 1        |
      | Tomorrow_+1 | 09:00 | 10:00 | COVID    | 5           | 1        |
      | Tomorrow_+2 | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following availability created event is created
      | Type     | By       | FromDate | ToDate      | Template_Days | FromTime | UntilTime | SlotLength | Capacity | Services |
      | Template | api@test | Tomorrow | Tomorrow_+2 | Relative      | 09:00    | 10:00     | 5          | 1        | COVID    |

  Scenario: Overwrites existing daily availability
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
    When I apply the following availability template
      | From     | Until    | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services |
      | Tomorrow | Tomorrow | Relative | 11:00    | 12:00     | 5          | 1        | COVID    |
    Then the request is successful and the following daily availability is created
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 11:00 | 12:00 | COVID    | 5           | 1        |