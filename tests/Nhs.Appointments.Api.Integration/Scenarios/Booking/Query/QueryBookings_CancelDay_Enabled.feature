Feature: Query for bookings

  Scenario: Get all bookings
    Given the following sessions
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID, FLU | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID, FLU | 10          | 1        |
    And the following bookings exist
      | Date              | Time  | Duration | Service |
      | Tomorrow          | 09:00 | 5        | COVID   |
      | 2 days from today | 09:20 | 10       | FLU     |
    When I query for bookings using the following parameters
      | From (Day) | From (Time) | Until (Day)       | Until (Time) |
      | Tomorrow   | 09:00       | 2 days from today | 09:20        |
    Then the following bookings are returned
      | Date              | Time  | Duration | Service |
      | Tomorrow          | 09:00 | 5        | COVID   |
      | 2 days from today | 09:20 | 10       | FLU     |

  Scenario: Query by time range (inclusive)
    Given the following sessions
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 10:00 | RSV:Adult | 10          | 5        |
    And the following bookings exist
      | Reference | Time  |
      | 1         | 09:00 |
      | 2         | 09:10 |
      | 3         | 09:20 |
      | 4         | 09:30 |
      | 5         | 09:40 |
    When I query for bookings using the following parameters
      | From (Day) | From (Time) | Until (Day) | Until (Time) |
      | Tomorrow   | 09:10       | Tomorrow    | 09:30        |
    Then the following bookings are returned
      | Date     | Time  | Duration | Service   | Reference |
      | Tomorrow | 09:10 | 10       | RSV:Adult | 2         |
      | Tomorrow | 09:20 | 10       | RSV:Adult | 3         |
      | Tomorrow | 09:30 | 10       | RSV:Adult | 4         |
