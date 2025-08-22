Feature: Joint Bookings (Disabled)

  Scenario: APPT-767 scenario one
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | RSV      | 5           | 1        |
      | Tomorrow | 09:00 | 10:00 | RSV      | 5           | 1        |
    When I check consecutive slot availability for 'RSV' between 'Tomorrow' and 'Tomorrow' with consecutive '2'
    Then the following availability is returned for 'Tomorrow'
      | From  | Until | Count |
      | 09:00 | 09:05 | 2     |
      | 09:05 | 09:10 | 2     |
      | 09:10 | 09:15 | 2     |
      | 09:15 | 09:20 | 2     |
      | 09:20 | 09:25 | 2     |
      | 09:25 | 09:30 | 2     |
      | 09:30 | 09:35 | 2     |
      | 09:35 | 09:40 | 2     |
      | 09:40 | 09:45 | 2     |
      | 09:45 | 09:50 | 2     |
      | 09:50 | 09:55 | 2     |
      | 09:55 | 10:00 | 2     |

  Scenario: APPT-767 scenario two
    Given the following sessions
      | Date     | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | RSV      | 5           | 1        |
      | Tomorrow | 09:00 | 10:00 | RSV      | 5           | 2        |
    When I check consecutive slot availability for 'RSV' between 'Tomorrow' and 'Tomorrow' with consecutive '2'
    Then the following availability is returned for 'Tomorrow'
      | From  | Until | Count |
      | 09:00 | 09:05 | 3     |
      | 09:05 | 09:10 | 3     |
      | 09:10 | 09:15 | 3     |
      | 09:15 | 09:20 | 3     |
      | 09:20 | 09:25 | 3     |
      | 09:25 | 09:30 | 3     |
      | 09:30 | 09:35 | 3     |
      | 09:35 | 09:40 | 3     |
      | 09:40 | 09:45 | 3     |
      | 09:45 | 09:50 | 3     |
      | 09:50 | 09:55 | 3     |
      | 09:55 | 10:00 | 3     |
