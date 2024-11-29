Feature: Create daily availability

  Scenario: Can apply an availability template
    Given there is no existing availability
    When I apply the following availability template
      | From     | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services | Mode     |
      | Tomorrow | 3 days from today | Relative | 09:00    | 10:00     | 5          | 1        | COVID    | Additive |
    Then the request is successful and the following daily availability sessions are created
      | Date              | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 3 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following availability created events are created
      | Type     | By       | FromDate | ToDate            | Template_Days | FromTime | UntilTime | SlotLength | Capacity | Services |
      | Template | api@test | Tomorrow | 3 days from today | Relative      | 09:00    | 10:00     | 5          | 1        | COVID    |

  Scenario: Overwrites existing daily availability
    Given the following sessions
      | Date              | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | COVID    | 5           | 2        |
    When I apply the following availability template
      | From     | Until             | Days     | TimeFrom | TimeUntil | SlotLength  | Capacity | Services | Mode      |
      | Tomorrow | 2 days from today | Relative | 11:00    | 12:00     | 10          | 1        | RSV      | Overwrite | 
    Then the request is successful and the following daily availability sessions are created
      | Date              | From  | Until | Services | Slot Length  | Capacity |
      | Tomorrow          | 11:00 | 12:00 | RSV      | 10           | 1        |
      | 2 days from today | 11:00 | 12:00 | RSV      | 10           | 1        |
    And the following availability created events are created
      | Type     | By       | FromDate | ToDate            | Template_Days | FromTime | UntilTime | SlotLength  | Capacity | Services |
      | Template | api@test | Tomorrow | 2 days from today | Relative      | 11:00    | 12:00     | 10          | 1        | RSV      |

  Scenario: Can add new sessions to existing availability
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 1        |
    When I apply the following availability template
      | From     | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services | Mode     |
      | Tomorrow | 2 days from today | Relative | 09:00    | 10:00     | 10         | 2        | COVID    | Additive |
    Then the request is successful and the following daily availability sessions are created
      | Date              | From  | Until | Services |  Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | COVID    | 5            | 1        |
      | Tomorrow          | 09:00 | 10:00 | COVID    | 10           | 2        |
      | 2 days from today | 09:00 | 10:00 | COVID    | 10           | 2        |
    And the following availability created events are created
      | Type     | By       | FromDate | ToDate            | Template_Days | FromTime | UntilTime | SlotLength  | Capacity | Services |
      | Template | api@test | Tomorrow | 2 days from today | Relative      | 09:00    | 10:00     | 10          | 2        | COVID    |

  Scenario: Can add new sessions to 2 different availability periods
    Given the following sessions
      | Date              | From  | Until | Services | Slot Length  | Capacity |
      | Tomorrow          | 09:00 | 17:00 | COVID    | 5            | 2        |
      | 2 days from today | 13:00 | 15:00 | RSV      | 10           | 1        |
    When I apply the following availability template
      | From     | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services | Mode     |
      | Tomorrow | 2 days from today | Relative | 09:00    | 10:00     | 15         | 3        | FLU      | Additive |
    Then the request is successful and the following daily availability sessions are created
      | Date              | From  | Until | Services |  Slot Length  | Capacity |
      | Tomorrow          | 09:00 | 17:00 | COVID    | 5             | 2        |
      | Tomorrow          | 09:00 | 10:00 | FLU      | 15            | 3        |
      | 2 days from today | 13:00 | 15:00 | RSV      | 10            | 1        |
      | 2 days from today | 09:00 | 10:00 | FLU      | 15            | 3        |
    And the following availability created events are created
      | Type     | By       | FromDate | ToDate            | Template_Days | FromTime | UntilTime | SlotLength  | Capacity | Services |
      | Template | api@test | Tomorrow | 2 days from today | Relative      | 09:00    | 10:00     | 15          | 3        | FLU      |
