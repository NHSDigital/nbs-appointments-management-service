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
    Then the booking with reference '68537-44913' has been 'Orphaned'

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
