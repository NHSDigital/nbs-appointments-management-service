Feature: Get all bookings for a person using NHS Number

  Scenario: Get all bookings for a patient
    Given the following sessions
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID, FLU | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID, FLU | 10          | 1        |
    And the following bookings have been made
      | Date              | Time  | Duration | Service |
      | Tomorrow          | 09:00 | 5        | COVID   |
      | 2 days from today | 09:20 | 10       | FLU     |
    When I query for bookings for a person using their NHS number
    Then the following bookings are returned
      | Date              | Time  | Duration | Service |
      | Tomorrow          | 09:00 | 5        | COVID   |
      | 2 days from today | 09:20 | 10       | FLU     |

  Scenario: Provisional bookings are not returned
    Given the following sessions
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID, FLU | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID, FLU | 10          | 1        |
    Given the following provisional bookings have been made
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:00 | 5        | COVID   |
    When I query for bookings for a person using their NHS number
    Then the request is successful and no bookings are returned

  Scenario: Provisional bookings are not returned but confirmed bookings are
    Given the following sessions
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID, FLU | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID, FLU | 10          | 1        |
    Given the following provisional bookings have been made
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:00 | 5        | COVID   |
    And the following bookings have been made
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:10 | 5        | COVID   |
    When I query for bookings for a person using their NHS number
    Then the following bookings are returned
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:10 | 5        | COVID   |

  Scenario: Returns success if no bookings are found for a person
    When I query for bookings for a person using their NHS number
    Then the request is successful and no bookings are returned