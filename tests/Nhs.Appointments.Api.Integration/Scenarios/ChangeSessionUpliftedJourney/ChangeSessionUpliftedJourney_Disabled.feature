Feature: Change Session Uplifted Journey

  Scenario: Change Session
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 68537-44913 |
    When I cancel the following session using the new endpoint
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    Then the call should fail with 501
