Feature: Set availability

  Scenario: Create availability for a single day
    Given there is no existing availability
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID    | Overwrite |
    Then the request is successful and the following daily availability sessions are created
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 1        |
    And the following availability created events are created
      | Type              | By       | FromDate | ToDate | Template_Days | FromTime | UntilTime | SlotLength | Capacity | Services |
      | SingleDateSession | api@test | Tomorrow |        |               | 09:00    | 17:00     | 5          | 1        | COVID    |

  Scenario: Overwrite existing availability for a single day
    Given the following sessions
      | Date              | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | COVID    | 5           | 2        |
    When I apply the following availability
      | Date     | From  | Until | SlotLength  | Capacity | Services | Mode      |
      | Tomorrow | 12:00 | 15:00 | 10          | 2        | FLU      | Overwrite |
    Then the request is successful and the following daily availability sessions are created
      | Date     | From  | Until | Services | Slot Length  | Capacity |
      | Tomorrow | 12:00 | 15:00 | FLU      | 10           | 2        |
    And the following availability created events are created
      | Type              | By       | FromDate | ToDate | Template_Days | FromTime | UntilTime | SlotLength  | Capacity | Services |
      | SingleDateSession | api@test | Tomorrow |        |               | 12:00    | 15:00     | 10          | 2        | FLU      |

  Scenario: Can add new sessions to existing availability for a single day
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
    When I apply the following availability
      | Date     | From  | Until | SlotLength  | Capacity | Services | Mode     |
      | Tomorrow | 12:00 | 15:00 | 10          | 2        | FLU      | Additive |
    Then the request is successful and the following daily availability sessions are created
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
      | Tomorrow | 12:00 | 15:00 | FLU      | 10          | 2        |
    And the following availability created events are created
      | Type              | By       | FromDate | ToDate | Template_Days | FromTime | UntilTime | SlotLength  | Capacity | Services |
      | SingleDateSession | api@test | Tomorrow |        |               | 12:00    | 15:00     | 10          | 2        | FLU      |