Feature: Set availability for multiple services

  Scenario: Create availability for a single day
    Given there is no existing availability
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services        | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID, FLU, RSV | Overwrite |
    Then the request is successful and the following daily availability sessions are created
      | Date     | From  | Until | Services        | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID, FLU, RSV | 5           | 1        |
    And the following availability created events are created
      | Type              | By       | FromDate | ToDate | Template_Days | FromTime | UntilTime | SlotLength | Capacity | Services        |
      | SingleDateSession | api@test | Tomorrow |        |               | 09:00    | 17:00     | 5          | 1        | COVID, FLU, RSV |
    And an audit function document was created for user 'api@test' and function 'SetAvailabilityFunction'

  Scenario: Overwrite existing availability for a single day, remove a service
    Given the following sessions
      | Date     | From  | Until | Services        | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID, FLU, RSV | 5           | 2        |
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services   | Mode      |
      | Tomorrow | 12:00 | 15:00 | 10         | 2        | COVID, RSV | Overwrite |
    Then the request is successful and the following daily availability sessions are created
      | Date     | From  | Until | Services        | Slot Length | Capacity |
      | Tomorrow | 12:00 | 15:00 | COVID, RSV      | 10          | 2        |
    And the following availability created events are created
      | Type              | By       | FromDate | ToDate | Template_Days | FromTime | UntilTime | SlotLength | Capacity | Services        |
      | SingleDateSession | api@test | Tomorrow |        |               | 12:00    | 15:00     | 10         | 2        | COVID, RSV      |
    And an audit function document was created for user 'api@test' and function 'SetAvailabilityFunction'

  Scenario: Overwrite existing availability for a single day, add services
    Given the following sessions
      | Date     | From  | Until | Services        | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID, FLU, RSV | 5           | 2        |
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services                            | Mode      |
      | Tomorrow | 12:00 | 15:00 | 10         | 2        | COVID, FLU, COVID-16, COVID-75, RSV | Overwrite |
    Then the request is successful and the following daily availability sessions are created
      | Date     | From  | Until | Services                                 | Slot Length | Capacity |
      | Tomorrow | 12:00 | 15:00 | COVID, FLU, COVID-16, COVID-75, RSV      | 10          | 2        |
    And the following availability created events are created
      | Type              | By       | FromDate | ToDate | Template_Days | FromTime | UntilTime | SlotLength | Capacity | Services                               |
      | SingleDateSession | api@test | Tomorrow |        |               | 12:00    | 15:00     | 10         | 2        | COVID, FLU, COVID-16, COVID-75, RSV    |
    And an audit function document was created for user 'api@test' and function 'SetAvailabilityFunction'
    
  Scenario: Greedy allocation alphabetical - suboptimal - Appointment status is recalculated after availability is created
    Given there is no existing availability
    And the following orphaned bookings exist
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 37492-16293 |
      | Tomorrow | 09:20 | 5        | FLU     | 89999-44622 |
      | Tomorrow | 09:20 | 5        | RSV     | 67834-56421 |
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services      | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID, RSV    | Overwrite |
    And I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services      | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID, FLU    | Additive |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'

  Scenario: Greedy allocation alphabetical - optimal - Appointment status is recalculated after availability is created
    Given there is no existing availability
    And the following orphaned bookings exist
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 37492-16293 |
      | Tomorrow | 09:20 | 5        | RSV     | 67834-56421 |
      | Tomorrow | 09:20 | 5        | FLU     | 89999-44622 |
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services      | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID, RSV    | Overwrite |
    And I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services      | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID, FLU    | Additive  |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'

#  Scenario: Greedy allocation alphabetical - shuffling - Appointment status is recalculated after availability is created
#    Given there is no existing availability
#    And the following orphaned bookings exist
#      | Date     | Time  | Duration | Service | Reference   |
#      | Tomorrow | 09:20 | 5        | COVID   | 37492-16293 |
#      | Tomorrow | 09:20 | 5        | RSV     | 67834-56421 |
#      | Tomorrow | 09:20 | 5        | FLU     | 89999-44622 |
#    When I apply the following availability
#      | Date     | From  | Until | SlotLength | Capacity | Services      | Mode      |
#      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID, RSV    | Overwrite |
#    And I apply the following availability
#      | Date     | From  | Until | SlotLength | Capacity | Services      | Mode      |
#      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID, FLU    | Additive  |
#    Then the booking with reference '37492-16293' has status 'Booked'
#    And the booking with reference '37492-16293' has availability status 'Supported'
#    Then the booking with reference '67834-56421' has status 'Booked'
#    And the booking with reference '67834-56421' has availability status 'Supported'
#    Then the booking with reference '89999-44622' has status 'Booked'
#    And the booking with reference '89999-44622' has availability status 'Orphaned'
#    When I apply the following availability
#      | Date     | From  | Until | SlotLength | Capacity | Services              | Mode      |
#      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID, ABC, RSV, FLU  | Additive  |
#    Then the booking with reference '37492-16293' has status 'Booked'
#    And the booking with reference '37492-16293' has availability status 'Supported'
#    Then the booking with reference '67834-56421' has status 'Booked'
#    And the booking with reference '67834-56421' has availability status 'Supported'
#    Then the booking with reference '89999-44622' has status 'Booked'
#    And the booking with reference '89999-44622' has availability status 'Orphaned'
  
#  Scenario: Edit an existing session
#    Given the following sessions
#      | Date     | From  | Until | Services | Slot Length | Capacity |
#      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
#    When I edit the following availability
#      | Date     | Old From | Old Until | Old SlotLength | Old Capacity | Old Services | New From | New Until | New SlotLength | New Capacity | New Services | Mode |
#      | Tomorrow | 09:00    | 17:00     | 5              | 2            | COVID        | 12:00    | 15:00     | 10             | 2            | FLU          | Edit |
#    Then the request is successful and the following daily availability sessions are created
#      | Date     | From  | Until | Services | Slot Length | Capacity |
#      | Tomorrow | 12:00 | 15:00 | FLU      | 10          | 2        |
#    And an audit function document was created for user 'api@test' and function 'SetAvailabilityFunction'
#
#  Scenario: Edit one of multiple sessions
#    Given the following sessions
#      | Date     | From  | Until | Services | Slot Length | Capacity |
#      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
#      | Tomorrow | 10:00 | 12:00 | FLU      | 10          | 1        |
#      | Tomorrow | 14:00 | 16:00 | FLU      | 10          | 1        |
#    When I edit the following availability
#      | Date     | Old From | Old Until | Old SlotLength | Old Capacity | Old Services | New From | New Until | New SlotLength | New Capacity | New Services | Mode |
#      | Tomorrow | 09:00    | 17:00     | 5              | 2            | COVID        | 12:00    | 15:00     | 10             | 2            | FLU          | Edit |
#    Then the request is successful and the following daily availability sessions are created
#      | Date     | From  | Until | Services | Slot Length | Capacity |
#      | Tomorrow | 12:00 | 15:00 | FLU      | 10          | 2        |
#      | Tomorrow | 10:00 | 12:00 | FLU      | 10          | 1        |
#      | Tomorrow | 14:00 | 16:00 | FLU      | 10          | 1        |
#    And an audit function document was created for user 'api@test' and function 'SetAvailabilityFunction'
#
#  Scenario: Edit one of two identical sessions
#    Given the following sessions
#      | Date     | From  | Until | Services | Slot Length | Capacity |
#      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
#      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
#    When I edit the following availability
#      | Date     | Old From | Old Until | Old SlotLength | Old Capacity | Old Services | New From | New Until | New SlotLength | New Capacity | New Services | Mode |
#      | Tomorrow | 09:00    | 17:00     | 5              | 2            | COVID        | 12:00    | 15:00     | 10             | 2            | FLU          | Edit |
#    Then the request is successful and the following daily availability sessions are created
#      | Date     | From  | Until | Services | Slot Length | Capacity |
#      | Tomorrow | 12:00 | 15:00 | FLU      | 10          | 2        |
#      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
#      And an audit function document was created for user 'api@test' and function 'SetAvailabilityFunction'
#
#  Scenario: Appointment status recalculation does not affect cancelled or provisional bookings
#    Given there is no existing availability
#    And the following cancelled bookings have been made
#      | Date     | Time  | Duration | Service | Reference   |
#      | Tomorrow | 09:20 | 5        | COVID   | 10293-45957 |
#    And the following provisional bookings have been made
#      | Date     | Time  | Duration | Service | Reference   |
#      | Tomorrow | 09:30 | 5        | COVID   | 48232-10293 |
#    When I apply the following availability
#      | Date     | From  | Until | SlotLength | Capacity | Services | Mode      |
#      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID    | Overwrite |
#    Then the booking with reference '10293-45957' has status 'Cancelled'
#    And the booking with reference '10293-45957' has availability status 'Unknown'
#    And the booking with reference '48232-10293' has status 'Provisional'
#    And the booking with reference '48232-10293' has availability status 'Supported'
#
#  Scenario: Provisional bookings are still considered live and prevent orphaned appointments taking their place
#    Given the following sessions
#      | Date     | From  | Until | Services | Slot Length | Capacity |
#      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
#    And the following bookings have been made
#      | Date     | Time  | Duration | Service | Reference   |
#      | Tomorrow | 09:20 | 5        | COVID   | 56923-19232 |
#    And the following provisional bookings have been made
#      | Date     | Time  | Duration | Service | Reference   |
#      | Tomorrow | 09:20 | 5        | COVID   | 19283-30492 |
#    And the following orphaned bookings exist
#      | Date     | Time  | Duration | Service | Reference   |
#      | Tomorrow | 09:20 | 5        | COVID   | 45721-10293 |
#    When I apply the following availability
#      | Date     | From  | Until | SlotLength | Capacity | Services | Mode      |
#      | Tomorrow | 09:00 | 17:00 | 5          | 2        | COVID    | Overwrite |
#    Then the booking with reference '56923-19232' has status 'Booked'
#    And the booking with reference '56923-19232' has availability status 'Supported'
#    And the booking with reference '19283-30492' has status 'Provisional'
#    And the booking with reference '19283-30492' has availability status 'Supported'
#    And the booking with reference '45721-10293' has status 'Booked'
#    And the booking with reference '45721-10293' has availability status 'Orphaned'
#
#  Scenario: Provisional bookings that are unsupported are deleted from the DB
#    Given the following sessions
#      | Date     | From  | Until | Services | Slot Length | Capacity |
#      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
#    And the following bookings have been made
#      | Date     | Time  | Duration | Service | Reference   |
#      | Tomorrow | 09:20 | 5        | COVID   | 84583-19232 |
#      | Tomorrow | 09:20 | 5        | COVID   | 90386-19232 |
#    And the following provisional bookings have been made
#      | Date     | Time  | Duration | Service | Reference   |
#      | Tomorrow | 09:20 | 5        | COVID   | 92225-30492 |
#    When I apply the following availability
#      | Date     | From  | Until | SlotLength | Capacity | Services | Mode      |
#      | Tomorrow | 09:00 | 17:00 | 5          | 2        | COVID    | Overwrite |
#    Then the booking with reference '84583-19232' has status 'Booked'
#    And the booking with reference '84583-19232' has availability status 'Supported'
#    And the booking with reference '90386-19232' has status 'Booked'
#    And the booking with reference '90386-19232' has availability status 'Supported'
#    And the booking with reference '92225-30492' should be deleted
#
#  Scenario: Expired provisional bookings are not considered live and can allow orphaned appointments to take their place
#    Given the following sessions
#      | Date     | From  | Until | Services | Slot Length | Capacity |
#      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
#    And the following bookings have been made
#      | Date     | Time  | Duration | Service | Reference   |
#      | Tomorrow | 09:20 | 5        | COVID   | 65734-19232 |
#    And the following expired provisional bookings have been made
#      | Date     | Time  | Duration | Service | Reference   |
#      | Tomorrow | 09:20 | 5        | COVID   | 92363-30492 |
#    And the following orphaned bookings exist
#      | Date     | Time  | Duration | Service | Reference   |
#      | Tomorrow | 09:20 | 5        | COVID   | 61865-10293 |
#    When I apply the following availability
#      | Date     | From  | Until | SlotLength | Capacity | Services | Mode      |
#      | Tomorrow | 09:00 | 17:00 | 5          | 2        | COVID    | Overwrite |
#    Then the booking with reference '65734-19232' has status 'Booked'
#    And the booking with reference '65734-19232' has availability status 'Supported'
##    The expired provisional stays in the DB, and will be cleaned up by the cleanup process
#    And the booking with reference '92363-30492' has status 'Provisional'
#    And the booking with reference '92363-30492' has availability status 'Supported'
#    And the booking with reference '61865-10293' has status 'Booked'
#    And the booking with reference '61865-10293' has availability status 'Supported'
#
#  Scenario: Bookings are prioritised by created date
#    Given there is no existing availability
#    And the following bookings have been made
#      | Date     | Time  | Duration | Service | Reference   | Created                  |
#      | Tomorrow | 09:20 | 5        | COVID   | 34482-10293 | 2024-12-01T09:00:00.000Z |
#    And the following orphaned bookings exist
#      | Date     | Time  | Duration | Service | Reference   | Created                  |
#      | Tomorrow | 09:20 | 5        | COVID   | 45853-10293 | 2024-11-03T09:00:00.000Z |
#    When I apply the following availability
#      | Date     | From  | Until | SlotLength | Capacity | Services | Mode      |
#      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID    | Overwrite |
#    Then the booking with reference '34482-10293' has status 'Booked'
#    And the booking with reference '34482-10293' has availability status 'Orphaned'
#    And the booking with reference '45853-10293' has status 'Booked'
#    And the booking with reference '45853-10293' has availability status 'Supported'
