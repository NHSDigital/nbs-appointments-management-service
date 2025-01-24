Feature: Get all bookings for a person using NHS Number

  Scenario: Get all bookings for a patient
    Given the following sessions
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID, FLU | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID, FLU | 10          | 1        |
    And the following bookings have been made
      | Date              | Time  | Duration | Service | Reference   | Created                  |
      | Tomorrow          | 09:00 | 5        | COVID   | 49581-39578 | 2024-12-01T09:00:00.000Z |
      | 2 days from today | 09:20 | 10       | FLU     | 58371-39584 | 2024-11-02T09:00:00.000Z |
    And the following orphaned bookings exist
      | Date              | Time  | Duration | Service | Reference   | Created                  |
      | 3 days from today | 09:20 | 5        | COVID   | 59283-59481 | 2024-12-03T09:00:00.000Z |
    When I query for bookings for a person using their NHS number
    Then the following bookings are returned
      | Date              | Time  | Duration | Service | Reference   | Status   | Created                  |
      | Tomorrow          | 09:00 | 5        | COVID   | 49581-39578 | Booked   | 2024-12-01T09:00:00.000Z |
      | 2 days from today | 09:20 | 10       | FLU     | 58371-39584 | Booked   | 2024-11-02T09:00:00.000Z |
      | 3 days from today | 09:20 | 5        | COVID   | 59283-59481 | Orphaned | 2024-12-03T09:00:00.000Z |

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

  Scenario: Cancelled bookings are not returned
    Given the following sessions
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID, FLU | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID, FLU | 10          | 1        |
    Given the following cancelled bookings have been made
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:00 | 5        | COVID   |
    When I query for bookings for a person using their NHS number
    Then the request is successful and no bookings are returned

  Scenario: Provisional and Cancelled bookings are not returned but confirmed and Orphaned bookings are
    Given the following sessions
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID, FLU | 5           | 1        |
      | 2 days from today | 09:00 | 10:00 | COVID, FLU | 10          | 1        |
    Given the following provisional bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:00 | 5        | COVID   | 10394-12039 |
    And the following bookings have been made
      | Date     | Time  | Duration | Service | Reference   | Created                  |
      | Tomorrow | 09:10 | 5        | COVID   | 39031-93231 | 2024-12-01T09:00:00.000Z |
    And the following orphaned bookings exist
      | Date              | Time  | Duration | Service | Reference   | Created                  |
      | 2 days from today | 09:20 | 5        | COVID   | 01923-76941 | 2024-12-03T09:00:00.000Z |
    And the following cancelled bookings have been made
      | Date     | Time  | Duration | Service | Reference   |
      | Tomorrow | 09:00 | 5        | COVID   | 50192-39381 |
    When I query for bookings for a person using their NHS number
    Then the following bookings are returned
      | Date              | Time  | Duration | Service | Reference   | Status   | Created                  |
      | Tomorrow          | 09:10 | 5        | COVID   | 39031-93231 | Booked   | 2024-12-01T09:00:00.000Z |
      | 2 days from today | 09:20 | 5        | COVID   | 01923-76941 | Orphaned | 2024-12-03T09:00:00.000Z |

  Scenario: Returns success if no bookings are found for a person
    When I query for bookings for a person using their NHS number
    Then the request is successful and no bookings are returned

