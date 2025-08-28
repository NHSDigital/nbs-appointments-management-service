Feature: Create daily availability for multiple services

  Scenario: Greedy allocation alphabetical - suboptimal - Appointment status is recalculated after an availability template is applied
    Given there is no existing availability
    And the following orphaned bookings exist
      | Date              | Time  | Duration | Service | Reference   |
      | 3 days from today | 09:20 | 5        | COVID   | 37492-16293 |
      | 3 days from today | 09:20 | 5        | FLU     | 89999-44622 |
      | 3 days from today | 09:20 | 5        | RSV     | 67834-56421 |
    When I apply the following availability template
      | From        | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services      | Mode      |
      | Tomorrow    | 5 days from today | Relative | 09:00    | 10:00     | 5          | 1        | COVID, RSV    | Overwrite  |
    When I apply the following availability template
      | From                | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services      | Mode        |
      | 2 days from today   | 7 days from today | Relative | 09:00    | 10:00     | 5          | 1        | COVID, FLU    | Additive    |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'

  Scenario: Greedy allocation alphabetical - optimal - Appointment status is recalculated after an availability template is applied
    Given there is no existing availability
    And the following orphaned bookings exist
      | Date              | Time  | Duration | Service | Reference   |
      | 3 days from today | 09:20 | 5        | COVID   | 37492-16293 |
      | 3 days from today | 09:20 | 5        | RSV     | 67834-56421 |
      | 3 days from today | 09:20 | 5        | FLU     | 89999-44622 |
    When I apply the following availability template
      | From        | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services      | Mode      |
      | Tomorrow    | 5 days from today | Relative | 09:00    | 10:00     | 5          | 1        | COVID, RSV    | Overwrite  |
    When I apply the following availability template
      | From                | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services      | Mode        |
      | 2 days from today   | 7 days from today | Relative | 09:00    | 10:00     | 5          | 1        | COVID, FLU    | Additive    |
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
      | Date              | Time  | Duration | Service | Reference   |
      | 3 days from today | 09:20 | 5        | B       | 37492-16293 |
      | 3 days from today | 09:20 | 5        | C       | 67834-56421 |
      | 3 days from today | 09:20 | 5        | E       | 89999-44622 |
    When I apply the following availability template
      | From        | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services      | Mode      |
      | Tomorrow    | 5 days from today | Relative | 09:00    | 17:00     | 5          | 2        | C, B, D, F    | Overwrite  |
    When I apply the following availability template
      | From                | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services      | Mode        |
      | 2 days from today   | 7 days from today | Relative | 09:00    | 17:00     | 5          | 2        | A, B, C, E    | Additive    |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'
    When I apply the following availability template
      | From                | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services      | Mode        |
      | 3 days from today   | 4 days from today | Relative | 09:00    | 17:00     | 5          | 2        | A, B, C, D    | Additive    |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Supported'

  Scenario: Greedy allocation service lengths - suboptimal - Appointment status is recalculated after an availability template is applied
    Given there is no existing availability
    And the following orphaned bookings exist
      | Date              | Time  | Duration | Service | Reference   |
      | 3 days from today | 09:20 | 5        | COVID   | 37492-16293 |
      | 3 days from today | 09:20 | 5        | FLU     | 89999-44622 |
      | 3 days from today | 09:20 | 5        | RSV     | 67834-56421 |
    When I apply the following availability template
      | From        | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services         | Mode      |
      | Tomorrow    | 5 days from today | Relative | 09:00    | 17:00     | 5          | 1        | ABBA, COVID, RSV | Overwrite  |
    When I apply the following availability template
      | From                | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services   | Mode      |
      | 2 days from today   | 7 days from today | Relative | 09:00    | 17:00     | 5          | 1        | COVID, FLU | Additive  |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'

  Scenario: Greedy allocation service lengths - optimal - Appointment status is recalculated after an availability template is applied
    Given there is no existing availability
    And the following orphaned bookings exist
      | Date              | Time  | Duration | Service | Reference   |
      | 3 days from today | 09:20 | 5        | COVID   | 37492-16293 |
      | 3 days from today | 09:20 | 5        | RSV     | 67834-56421 |
      | 3 days from today | 09:20 | 5        | FLU     | 89999-44622 |
    When I apply the following availability template
      | From        | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services         | Mode      |
      | Tomorrow    | 5 days from today | Relative | 09:00    | 17:00     | 5          | 1        | ABBA, COVID, RSV | Overwrite  |
    When I apply the following availability template
      | From                | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services   | Mode      |
      | 2 days from today   | 7 days from today | Relative | 09:00    | 17:00     | 5          | 1        | COVID, FLU | Additive  |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'

  # Prove that creating more availability for B and C results in booking E being supported, through service length shuffling
  Scenario: Greedy allocation service lengths - shuffling - Appointment is supported after an unrelated availability template is applied
    Given there is no existing availability
    And the following orphaned bookings exist
      | Date              | Time  | Duration | Service | Reference   |
      | 3 days from today | 09:20 | 5        | B       | 37492-16293 |
      | 3 days from today | 09:20 | 5        | C       | 67834-56421 |
      | 3 days from today | 09:20 | 5        | E       | 89999-44622 |
    When I apply the following availability template
      | From        | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services            | Mode      |
      | Tomorrow    | 5 days from today | Relative | 09:00    | 17:00     | 5          | 2        | A, B, C, D, F, G, H | Overwrite  |
    When I apply the following availability template
      | From                | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services    | Mode      |
      | 2 days from today   | 7 days from today | Relative | 09:00    | 17:00     | 5          | 2        | A, B, C, E  | Additive  |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Orphaned'
    When I apply the following availability template
      | From                | Until             | Days     | TimeFrom | TimeUntil | SlotLength | Capacity | Services    | Mode      |
      | 3 days from today   | 4 days from today | Relative | 09:00    | 17:00     | 5          | 2        | B, C        | Additive  |
    Then the booking with reference '37492-16293' has status 'Booked'
    And the booking with reference '37492-16293' has availability status 'Supported'
    Then the booking with reference '67834-56421' has status 'Booked'
    And the booking with reference '67834-56421' has availability status 'Supported'
    Then the booking with reference '89999-44622' has status 'Booked'
    And the booking with reference '89999-44622' has availability status 'Supported'
