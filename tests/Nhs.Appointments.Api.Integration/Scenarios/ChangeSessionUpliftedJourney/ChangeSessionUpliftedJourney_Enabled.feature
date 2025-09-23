Feature: Change Session Uplifted Journey

  Scenario: Change Session Cancels All Sessions In A Single Day
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 68537-44913 |
    When I cancel all sessions in between 'Tomorrow' and 'Tomorrow'
    Then the booking with reference '68537-44913' has been 'Cancelled'
    And there are no sessions for 'Tomorrow'

  Scenario: Change Session Cancels All Sessions Over Multiple Days
    Given the following sessions
      | Date              | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date              | Time  | Duration | Service | Reference   |
      | Tomorrow          | 09:45 | 5        | COVID   | 68537-44913 |
      | 2 days from today | 09:20 | 5        | COVID   | 12345-09876 |
    When I cancel all sessions in between 'Tomorrow' and '2 days from today'
    Then the booking with reference '68537-44913' has been 'Cancelled'
    And the booking with reference '12345-09876' has been 'Cancelled'
    And there are no sessions for 'Tomorrow'
    And there are no sessions for '2 days from today'
