Feature: Cancel a session

  Scenario: Cancel a session
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:30 | 5        | COVID   | 68374-29374 |
    When I cancel the following session
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 17:00 | COVID    | 5           | 2        |
    Then the booking with reference '68374-29374' has been 'Orphaned'
