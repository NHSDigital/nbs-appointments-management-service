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
    And an audit function document was created for user 'api@test' and function 'SetAvailabilityFunction'

  Scenario: Overwrite existing availability for a single day
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services | Mode      |
      | Tomorrow | 12:00 | 15:00 | 10         | 2        | FLU      | Overwrite |
    Then the request is successful and the following daily availability sessions are created
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 12:00 | 15:00 | FLU      | 10          | 2        |
    And the following availability created events are created
      | Type              | By       | FromDate | ToDate | Template_Days | FromTime | UntilTime | SlotLength | Capacity | Services |
      | SingleDateSession | api@test | Tomorrow |        |               | 12:00    | 15:00     | 10         | 2        | FLU      |
    And an audit function document was created for user 'api@test' and function 'SetAvailabilityFunction'

  Scenario: Can add new sessions to existing availability for a single day
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services | Mode     |
      | Tomorrow | 12:00 | 15:00 | 10         | 2        | FLU      | Additive |
    Then the request is successful and the following daily availability sessions are created
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
      | Tomorrow | 12:00 | 15:00 | FLU      | 10          | 2        |
    And the following availability created events are created
      | Type              | By       | FromDate | ToDate | Template_Days | FromTime | UntilTime | SlotLength | Capacity | Services |
      | SingleDateSession | api@test | Tomorrow |        |               | 12:00    | 15:00     | 10         | 2        | FLU      |
    And an audit function document was created for user 'api@test' and function 'SetAvailabilityFunction'

  Scenario: Appointment status is recalculated after availability is created
    Given there is no existing availability
    And the following orphaned bookings exist
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 37492-16293 |
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID    | Overwrite |
    Then the booking with reference '37492-16293' has been 'Booked'
    And an audit function document was created for user 'api@test' and function 'SetAvailabilityFunction'

  Scenario: Edit an existing session
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
    When I edit the following availability
      | Date     | Old From | Old Until | Old SlotLength | Old Capacity | Old Services | New From | New Until | New SlotLength | New Capacity | New Services | Mode |
      | Tomorrow | 09:00    | 17:00     | 5              | 2            | COVID        | 12:00    | 15:00     | 10             | 2            | FLU          | Edit |
    Then the request is successful and the following daily availability sessions are created
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 12:00 | 15:00 | FLU      | 10          | 2        |
    And an audit function document was created for user 'api@test' and function 'SetAvailabilityFunction'

  Scenario: Edit one of multiple sessions
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
      | Tomorrow | 10:00 | 12:00 | FLU      | 10          | 1        |
      | Tomorrow | 14:00 | 16:00 | FLU      | 10          | 1        |
    When I edit the following availability
      | Date     | Old From | Old Until | Old SlotLength | Old Capacity | Old Services | New From | New Until | New SlotLength | New Capacity | New Services | Mode |
      | Tomorrow | 09:00    | 17:00     | 5              | 2            | COVID        | 12:00    | 15:00     | 10             | 2            | FLU          | Edit |
    Then the request is successful and the following daily availability sessions are created
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 12:00 | 15:00 | FLU      | 10          | 2        |
      | Tomorrow | 10:00 | 12:00 | FLU      | 10          | 1        |
      | Tomorrow | 14:00 | 16:00 | FLU      | 10          | 1        |
    And an audit function document was created for user 'api@test' and function 'SetAvailabilityFunction'

  Scenario: Edit one of two identical sessions
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
    When I edit the following availability
      | Date     | Old From | Old Until | Old SlotLength | Old Capacity | Old Services | New From | New Until | New SlotLength | New Capacity | New Services | Mode |
      | Tomorrow | 09:00    | 17:00     | 5              | 2            | COVID        | 12:00    | 15:00     | 10             | 2            | FLU          | Edit |
    Then the request is successful and the following daily availability sessions are created
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 12:00 | 15:00 | FLU      | 10          | 2        |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
    And an audit function document was created for user 'api@test' and function 'SetAvailabilityFunction'
