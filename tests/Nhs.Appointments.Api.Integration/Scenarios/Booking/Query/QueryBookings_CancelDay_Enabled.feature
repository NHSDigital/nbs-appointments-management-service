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
      | From     | At    | To                | At    |
      | Tomorrow | 09:00 | 2 days from today | 09:20 |
    Then the following bookings are returned
      | Date              | Time  | Duration | Service |
      | Tomorrow          | 09:00 | 5        | COVID   |
      | 2 days from today | 09:20 | 10       | FLU     |
