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
    And the following provisional bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:30 | 5        | COVID   | 79237-10283 |
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID    | Overwrite |
    Then the booking with reference '37492-16293' has been 'Booked'
    And the booking with reference '79237-10283' has status 'Provisional'
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

  Scenario: Appointment status recalculation does not affect cancelled or provisional bookings
    Given there is no existing availability
    And the following cancelled bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 10293-45957 |
    And the following provisional bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:30 | 5        | COVID   | 48232-10293 |
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID    | Overwrite |
    Then the booking with reference '10293-45957' has status 'Cancelled'
    And the booking with reference '48232-10293' has status 'Provisional'

  Scenario: Provisional bookings are still considered live and prevent orphaned appointments taking their place
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   | Created                  |
      | Tomorrow | 09:20 | 5        | COVID   | 56923-19232 | 2024-12-01T09:00:00.000Z |
    And the following provisional bookings have been made
      | Date     | Time  | Duration | Service | Reference   | Created                  |
      | Tomorrow | 09:20 | 5        | COVID   | 19283-30492 | 2024-12-02T09:00:00.000Z |
    And the following orphaned bookings exist
      | Date     | Time  | Duration | Service | Reference   | Created                  |
      | Tomorrow | 09:20 | 5        | COVID   | 45721-10293 | 2024-12-03T09:00:00.000Z |
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 2        | COVID    | Overwrite |
    Then the booking with reference '56923-19232' has status 'Booked'
    And the booking with reference '19283-30492' has status 'Provisional'
    And the booking with reference '45721-10293' has status 'Orphaned'

  Scenario: Bookings are prioritised by created date
    Given there is no existing availability
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   | Created                  |
      | Tomorrow | 09:20 | 5        | COVID   | 34482-10293 | 2024-12-01T09:00:00.000Z |
    And the following orphaned bookings exist
      | Date     | Time  | Duration | Service | Reference   | Created                  |
      | Tomorrow | 09:20 | 5        | COVID   | 45853-10293 | 2024-11-03T09:00:00.000Z |
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID    | Overwrite |
    Then the booking with reference '34482-10293' has status 'Orphaned'
    And the booking with reference '45853-10293' has status 'Booked'
