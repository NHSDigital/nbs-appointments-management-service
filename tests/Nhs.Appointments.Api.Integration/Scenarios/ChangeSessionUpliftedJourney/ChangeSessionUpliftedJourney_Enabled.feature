Feature: Change Session Uplifted Journey

# Wildcard cancellation not yet implemented
  @ignore 
  Scenario: Cancels All Sessions In A Single Day
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 68537-44913 |
    When I cancel all sessions in between 'Tomorrow' and 'Tomorrow'
    Then the booking with reference '68537-44913' has been 'Cancelled'
    And there are no sessions for 'Tomorrow'

# Wildcard cancellation not yet implemented
  @ignore
  Scenario: Cancels All Sessions Over Multiple Days
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

Scenario: Edit a single session on a single day orphans newly orphaned booking
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 68537-44913 |
    When I replace the session with the following and set cancelNewlyOrphanedBookings to 'false'
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 12:00 | 17:00 | COVID    | 5           | 1        |
    Then the session 'Tomorrow' should have been updated
    Then the booking with reference '68537-44913' has status 'Booked'
    Then the booking with reference '68537-44913' has availability status 'Orphaned'

  Scenario: Edit a single session on a single day cancels newly orphaned booking
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 68537-44913 |
    When  I replace the session with the following and set cancelNewlyOrphanedBookings to 'true'
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 12:00 | 17:00 | COVID    | 5           | 1        |
    Then the session 'Tomorrow' should have been updated
    Then the booking with reference '68537-44913' has status 'Booked'
    Then the booking with reference '68537-44913' has availability status 'Cancelled'

  Scenario: Cancel a single session on a single day
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
      | Tomorrow | 09:00 | 16:00 | RSV      | 10          | 2        |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:45 | 5        | COVID   | 68537-44913 |
    When I cancel the following session using the new endpoint and set cancelNewlyOrphanedBookings to 'false'
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
    Then the session 'Tomorrow' no longer exists
    Then the booking with reference '68537-44913' has status 'Booked'
    Then the booking with reference '68537-44913' has availability status 'Orphaned'

  Scenario: Cancel multiple sessions over multiple days
    Given the following sessions
      | Date              | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 3 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date              | Time  | Duration | Service | Reference   |
      | Tomorrow          | 09:45 | 5        | COVID   | 68537-44913 |
      | 2 days from today | 09:45 | 5        | COVID   | 12345-12345 |
      | 3 days from today | 09:45 | 5        | COVID   | 54321-54321 |
    When I cancel the sessions matching this between 'Tomorrow' and '3 days from now' and set cancelNewlyOrphanedBookings to 'false'
      | From  | Until | Services | Slot Length | Capacity |
      | 09:00 | 10:00 | COVID    | 5           | 1        |
    Then the sessions between 'Tomorrow' and '3 days from now' no longer exist
    And the booking with reference '68537-44913' has status 'Booked'
    And the booking with reference '68537-44913' has availability status 'Orphaned'
    Then the booking with reference '12345-12345' has status 'Booked'
    And the booking with reference '12345-12345' has availability status 'Orphaned'
    Then the booking with reference '54321-54321' has status 'Booked'
    And the booking with reference '54321-54321' has availability status 'Orphaned'

  Scenario: Edit multiple sessions over multiple days
    Given the following sessions
      | Date              | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 3 days from today | 09:00 | 10:00 | COVID    | 5           | 1        |
    And the following bookings have been made
      | Date              | Time  | Duration | Service | Reference   |
      | Tomorrow          | 09:45 | 5        | COVID   | 68537-44913 |
      | 2 days from today | 09:45 | 5        | COVID   | 12345-12345 |
      | 3 days from today | 09:45 | 5        | COVID   | 54321-54321 |
    When I replace multiple sessions between 'Tomorrow' and '3 days from now' with this session and set cancelNewlyOrphanedBookings to 'false'
      | From  | Until | Services | Slot Length | Capacity |
      | 11:00 | 16:00 | COVID    | 5           | 1        |
    Then the sessions between 'Tomorrow' and '3 days from now' should have been updated
    And the booking with reference '68537-44913' has status 'Booked'
    And the booking with reference '68537-44913' has availability status 'Orphaned'
    Then the booking with reference '12345-12345' has status 'Booked'
    And the booking with reference '12345-12345' has availability status 'Orphaned'
    Then the booking with reference '54321-54321' has status 'Booked'
    And the booking with reference '54321-54321' has availability status 'Orphaned'
