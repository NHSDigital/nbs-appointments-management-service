Feature: Cancel a session

  Scenario: Cancel the only session on a day
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 68537-44913 |
    When I cancel the following session
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    Then the booking with reference '68537-44913' has status 'Booked'
    Then the booking with reference '68537-44913' has availability status 'Orphaned'

  Scenario: Cancel one of two identical sessions
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 97531-43576 |
    When I cancel the following session
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    When I query for a booking with the reference number '97531-43576'
    Then the following booking is returned
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 97531-43576 |

  Scenario: Cancelling a session deletes Provisional appointments within it
    And the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following provisional bookings have been made
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:00 | 5        | COVID   |
    When I cancel the following session
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    Then the booking should be deleted

  Scenario: Cancel one of two identical sessions that have same services in different order
    Given the following sessions
      | Date     | From  | Until | Services      | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID, RSV    | 5           | 1        |
      | Tomorrow | 09:00 | 10:00 | RSV, COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 97531-43576 |
    When I cancel the following session
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    When I query for a booking with the reference number '97531-43576'
    Then the following booking is returned
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 97531-43576 |

  Scenario: Greedy allocation alphabetical - suboptimal - Appointment status for booked is recalculated after availability is cancelled
    Given the following sessions
      | Date     | From  | Until | Services   | Slot Length | Capacity  |
      | Tomorrow | 09:00 | 17:00 | COVID, RSV | 5           | 1         |
      | Tomorrow | 09:00 | 17:00 | RSV, FLU   | 5           | 1         |
      | Tomorrow | 09:00 | 17:00 | COVID, FLU | 5           | 1         |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 37492-16293 |
      | Tomorrow | 09:20 | 5        | FLU     | 89999-44622 |
      | Tomorrow | 09:20 | 5        | RSV     | 67834-56421 |
    When I cancel the following session
      | Date     | From  | Until | Services     | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | RSV, FLU     | 5           | 1        |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'

  Scenario: Greedy allocation alphabetical - suboptimal - Appointment status for provisional is recalculated and deleted after availability is cancelled
    Given the following sessions
      | Date     | From  | Until | Services   | Slot Length | Capacity  |
      | Tomorrow | 09:00 | 17:00 | COVID, RSV | 5           | 1         |
      | Tomorrow | 09:00 | 17:00 | RSV, FLU   | 5           | 1         |
      | Tomorrow | 09:00 | 17:00 | COVID, FLU | 5           | 1         |
    And the following provisional bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 37492-16293 |
      | Tomorrow | 09:20 | 5        | FLU     | 89999-44622 |
      | Tomorrow | 09:20 | 5        | RSV     | 67834-56421 |
    When I cancel the following session
      | Date     | From  | Until | Services     | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | RSV, FLU     | 5           | 1        |
    Then the booking with reference '37492-16293' has status 'Provisional'
    And the booking with reference '37492-16293' has availability status 'Supported'
    And the booking with reference '89999-44622' should be deleted
    Then the booking with reference '67834-56421' has status 'Provisional'
    And the booking with reference '67834-56421' has availability status 'Supported'

  Scenario: Greedy allocation alphabetical - optimal - Appointment status for booked is recalculated after availability is cancelled
    Given the following sessions
      | Date     | From  | Until | Services   | Slot Length | Capacity  |
      | Tomorrow | 09:00 | 17:00 | COVID, RSV | 5           | 1         |
      | Tomorrow | 09:00 | 17:00 | RSV, FLU   | 5           | 1         |
      | Tomorrow | 09:00 | 17:00 | COVID, FLU | 5           | 1         |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 37492-16293 |
      | Tomorrow | 09:20 | 5        | RSV     | 67834-56421 |
      | Tomorrow | 09:20 | 5        | FLU     | 89999-44622 |
    When I cancel the following session
      | Date     | From  | Until | Services     | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | RSV, FLU     | 5           | 1        |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'

  Scenario: Greedy allocation alphabetical - optimal - Appointment status for provisional is recalculated and deleted after availability is cancelled
    Given the following sessions
      | Date     | From  | Until | Services   | Slot Length | Capacity  |
      | Tomorrow | 09:00 | 17:00 | COVID, RSV | 5           | 1         |
      | Tomorrow | 09:00 | 17:00 | RSV, FLU   | 5           | 1         |
      | Tomorrow | 09:00 | 17:00 | COVID, FLU | 5           | 1         |
    And the following provisional bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 37492-16293 |
      | Tomorrow | 09:20 | 5        | RSV     | 67834-56421 |
      | Tomorrow | 09:20 | 5        | FLU     | 89999-44622 |
    When I cancel the following session
      | Date     | From  | Until | Services     | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | RSV, FLU     | 5           | 1        |
    Then the booking with reference '37492-16293' has status 'Provisional'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Provisional'
    And the booking with reference '67834-56421' has availability status 'Supported'
    And the booking with reference '89999-44622' should be deleted
    
  # Prove that cancelling availability for B and C results in booking E being orphaned, through alphabetical shuffling
  Scenario: Greedy allocation alphabetical - shuffling - Booked appointment is orphaned after unrelated availability is cancelled
    Given the following sessions
      | Date     | From  | Until | Services     | Slot Length | Capacity  |
      | Tomorrow | 09:00 | 17:00 | C, B, D, F   | 5           | 2         |
      | Tomorrow | 09:00 | 17:00 | A, B, C, E   | 5           | 2         |
      | Tomorrow | 09:00 | 17:00 | A, B, C, D   | 5           | 2         |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | B       | 37492-16293 |
      | Tomorrow | 09:20 | 5        | C       | 67834-56421 |
      | Tomorrow | 09:20 | 5        | E       | 89999-44622 |
    When I cancel the following session
      | Date     | From  | Until | Services     | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | A, B, C, D   | 5           | 2        |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'
  
  # Prove that cancelling availability for B and C results in booking E being deleted, through alphabetical shuffling
  Scenario: Greedy allocation alphabetical - shuffling - Provisional appointment is deleted after unrelated availability is cancelled
    Given the following sessions
      | Date     | From  | Until | Services     | Slot Length | Capacity  |
      | Tomorrow | 09:00 | 17:00 | C, B, D, F   | 5           | 2         |
      | Tomorrow | 09:00 | 17:00 | A, B, C, E   | 5           | 2         |
      | Tomorrow | 09:00 | 17:00 | A, B, C, D   | 5           | 2         |
    And the following provisional bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | B       | 37492-16293 |
      | Tomorrow | 09:20 | 5        | C       | 67834-56421 |
      | Tomorrow | 09:20 | 5        | E       | 89999-44622 |
    When I cancel the following session
      | Date     | From  | Until | Services     | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | A, B, C, D   | 5           | 2        |
    Then the booking with reference '37492-16293' has status 'Provisional'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Provisional'
    And the booking with reference '67834-56421' has availability status 'Supported'
    And the booking with reference '89999-44622' should be deleted

  Scenario: Greedy allocation service lengths - suboptimal - Appointment status for booked is recalculated after availability is cancelled
    Given the following sessions
      | Date     | From  | Until | Services                | Slot Length | Capacity  |
      | Tomorrow | 09:00 | 17:00 | ABBA, FLU, DELTA, FLU-B | 5           | 1         |
      | Tomorrow | 09:00 | 17:00 | ABBA, COVID, RSV        | 5           | 1         |
      | Tomorrow | 09:00 | 17:00 | COVID, FLU              | 5           | 1         |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 37492-16293 |
      | Tomorrow | 09:20 | 5        | FLU     | 89999-44622 |
      | Tomorrow | 09:20 | 5        | RSV     | 67834-56421 |
    When I cancel the following session
      | Date     | From  | Until | Services                    | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | ABBA, FLU, DELTA, FLU-B     | 5           | 1        |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'

  Scenario: Greedy allocation service lengths - suboptimal - Appointment status for provisional is deleted after availability is cancelled
    Given the following sessions
      | Date     | From  | Until | Services                | Slot Length | Capacity  |
      | Tomorrow | 09:00 | 17:00 | ABBA, FLU, DELTA, FLU-B | 5           | 1         |
      | Tomorrow | 09:00 | 17:00 | ABBA, COVID, RSV        | 5           | 1         |
      | Tomorrow | 09:00 | 17:00 | COVID, FLU              | 5           | 1         |
    And the following provisional bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 37492-16293 |
      | Tomorrow | 09:20 | 5        | FLU     | 89999-44622 |
      | Tomorrow | 09:20 | 5        | RSV     | 67834-56421 |
    When I cancel the following session
      | Date     | From  | Until | Services                    | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | ABBA, FLU, DELTA, FLU-B     | 5           | 1        |
    Then the booking with reference '37492-16293' has status 'Provisional'
    And the booking with reference '37492-16293' has availability status 'Supported'
    And the booking with reference '89999-44622' should be deleted
    Then the booking with reference '67834-56421' has status 'Provisional'
    And the booking with reference '67834-56421' has availability status 'Supported'

  Scenario: Greedy allocation service lengths - optimal - Appointment status for booked is recalculated after availability is cancelled
    Given the following sessions
      | Date     | From  | Until | Services                | Slot Length | Capacity  |
      | Tomorrow | 09:00 | 17:00 | ABBA, FLU, DELTA, FLU-B | 5           | 1         |
      | Tomorrow | 09:00 | 17:00 | ABBA, COVID, RSV        | 5           | 1         |
      | Tomorrow | 09:00 | 17:00 | COVID, FLU              | 5           | 1         |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 37492-16293 |
      | Tomorrow | 09:20 | 5        | RSV     | 67834-56421 |
      | Tomorrow | 09:20 | 5        | FLU     | 89999-44622 |
    When I cancel the following session
      | Date     | From  | Until | Services                    | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | ABBA, FLU, DELTA, FLU-B     | 5           | 1        |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'

  Scenario: Greedy allocation service lengths - optimal - Appointment status for provisional is recalculated and deleted after availability is cancelled
    Given the following sessions
      | Date     | From  | Until | Services                | Slot Length | Capacity  |
      | Tomorrow | 09:00 | 17:00 | ABBA, FLU, DELTA, FLU-B | 5           | 1         |
      | Tomorrow | 09:00 | 17:00 | ABBA, COVID, RSV        | 5           | 1         |
      | Tomorrow | 09:00 | 17:00 | COVID, FLU              | 5           | 1         |
    And the following provisional bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 37492-16293 |
      | Tomorrow | 09:20 | 5        | RSV     | 67834-56421 |
      | Tomorrow | 09:20 | 5        | FLU     | 89999-44622 |
    When I cancel the following session
      | Date     | From  | Until | Services                    | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | ABBA, FLU, DELTA, FLU-B     | 5           | 1        |
    Then the booking with reference '37492-16293' has status 'Provisional'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Provisional'
    And the booking with reference '67834-56421' has availability status 'Supported'
    And the booking with reference '89999-44622' should be deleted
  
   # Prove that cancelling availability for B and C results in booking E being orphaned, through service length shuffling
  Scenario: Greedy allocation service lengths - shuffling - Booked appointment is orphaned after unrelated availability is cancelled
    Given the following sessions
      | Date     | From  | Until | Services              | Slot Length | Capacity  |
      | Tomorrow | 09:00 | 17:00 | A, B, C, D, F, G, H   | 5           | 2         |
      | Tomorrow | 09:00 | 17:00 | A, B, C, E            | 5           | 2         |
      | Tomorrow | 09:00 | 17:00 | B, C                  | 5           | 2         |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | B       | 37492-16293 |
      | Tomorrow | 09:20 | 5        | C       | 67834-56421 |
      | Tomorrow | 09:20 | 5        | E       | 89999-44622 |
    When I cancel the following session
      | Date     | From  | Until | Services     | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | B, C         | 5           | 2        |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'
  
  # Prove that cancelling availability for B and C results in booking E being deleted, through service length shuffling
  Scenario: Greedy allocation service lengths - shuffling - Provisional appointment is deleted after unrelated availability is cancelled
    Given the following sessions
      | Date     | From  | Until | Services              | Slot Length | Capacity  |
      | Tomorrow | 09:00 | 17:00 | A, B, C, D, F, G, H   | 5           | 2         |
      | Tomorrow | 09:00 | 17:00 | A, B, C, E            | 5           | 2         |
      | Tomorrow | 09:00 | 17:00 | B, C                  | 5           | 2         |
    And the following provisional bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | B       | 37492-16293 |
      | Tomorrow | 09:20 | 5        | C       | 67834-56421 |
      | Tomorrow | 09:20 | 5        | E       | 89999-44622 |
    When I cancel the following session
      | Date     | From  | Until | Services     | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | B, C         | 5           | 2        |
    Then the booking with reference '37492-16293' has status 'Provisional'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Provisional'
    And the booking with reference '67834-56421' has availability status 'Supported'
    And the booking with reference '89999-44622' should be deleted
