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

  Scenario: Edit existing availability for a single day, remove a service
    Given the following sessions
      | Date     | From  | Until | Services        | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID, FLU, RSV | 5           | 2        |
    When I edit the following availability
      | Date     | Old From | Old Until | Old SlotLength | Old Capacity | Old Services     | New From | New Until | New SlotLength | New Capacity | New Services | Mode |
      | Tomorrow | 09:00    | 17:00     | 5              | 2            | COVID, FLU, RSV  | 09:00    | 17:00     | 5              | 2            | COVID, RSV   | Edit |
    Then the request is successful and the following daily availability sessions are created
      | Date     | From  | Until | Services        | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID, RSV      | 5           | 2        |
    And the following availability created events are created
      | Type              | By       | FromDate | ToDate | Template_Days | FromTime | UntilTime | SlotLength | Capacity | Services        |
      | SingleDateSession | api@test | Tomorrow |        |               | 09:00    | 17:00     | 5          | 2        | COVID, RSV      |
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

  # Prove that creating more availability for B and C results in booking E being supported, through alphabetical shuffling 
  Scenario: Greedy allocation alphabetical - shuffling - Appointment is supported after unrelated availability is created
    Given there is no existing availability
    And the following orphaned bookings exist
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | B       | 37492-16293 |
      | Tomorrow | 09:20 | 5        | C       | 67834-56421 |
      | Tomorrow | 09:20 | 5        | E       | 89999-44622 |
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services      | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 2        | C, B, D, F    | Overwrite |
    And I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services      | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 2        | A, B, C, E    | Additive  |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services    | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 2        | A, B, C, D  | Additive  |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Supported'

  Scenario: Greedy allocation service lengths - suboptimal - Appointment status is recalculated after availability is created
    Given there is no existing availability
    And the following orphaned bookings exist
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 37492-16293 |
      | Tomorrow | 09:20 | 5        | FLU     | 89999-44622 |
      | Tomorrow | 09:20 | 5        | RSV     | 67834-56421 |
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services          | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 1        | ABBA, COVID, RSV  | Overwrite |
    And I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services      | Mode     |
      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID, FLU    | Additive |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'

  Scenario: Greedy allocation service lengths - optimal - Appointment status is recalculated after availability is created
    Given there is no existing availability
    And the following orphaned bookings exist
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | COVID   | 37492-16293 |
      | Tomorrow | 09:20 | 5        | RSV     | 67834-56421 |
      | Tomorrow | 09:20 | 5        | FLU     | 89999-44622 |
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services          | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 1        | ABBA, COVID, RSV  | Overwrite |
    And I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services      | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 1        | COVID, FLU    | Additive  |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'

  # Prove that creating more availability for B and C results in booking E being supported, through service length shuffling
  Scenario: Greedy allocation service lengths - shuffling - Appointment is supported after unrelated availability is created
    Given there is no existing availability
    And the following orphaned bookings exist
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:20 | 5        | B       | 37492-16293 |
      | Tomorrow | 09:20 | 5        | C       | 67834-56421 |
      | Tomorrow | 09:20 | 5        | E       | 89999-44622 |
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services              | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 2        | A, B, C, D, F, G, H   | Overwrite |
    And I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services      | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 2        | A, B, C, E    | Additive  |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'
    When I apply the following availability
      | Date     | From  | Until | SlotLength | Capacity | Services  | Mode      |
      | Tomorrow | 09:00 | 17:00 | 5          | 2        | B, C      | Additive  |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Supported'
