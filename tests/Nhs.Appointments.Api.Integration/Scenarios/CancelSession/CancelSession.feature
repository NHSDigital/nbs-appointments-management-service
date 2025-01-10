Feature: Cancel a session

  Scenario: Cancel a session
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 68537-45913 |
    When I cancel the following session
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    Then the booking with reference '68537-45913' has been 'Orphaned'
