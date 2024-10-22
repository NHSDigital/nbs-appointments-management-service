Feature: Get all bookings for a person using NHS Number

    Background: 
        Given the following sessions
            | Date       | From  | Until | Services   | Slot Length | Capacity |
            | 2077-01-01 | 09:00 | 10:00 | COVID, FLU | 5           | 1        |
            | 2077-01-02 | 09:00 | 10:00 | COVID, FLU | 10          | 1        |

    Scenario: Get all bookings for a patient
        Given the following bookings have been made
            | Date       | Time  | Duration | Service | 
            | 2077-01-01 | 09:00 | 5        | COVID   |
            | 2077-01-02 | 09:20 | 10       | FLU     |
        When I query for bookings for a person using their NHS number 
        Then the following bookings are returned
          | Date       | Time  | Duration | Service |
          | 2077-01-01 | 09:00 | 5        | COVID   |
          | 2077-01-02 | 09:20 | 10       | FLU     |

    Scenario: Returns success if no bookings are found for a person 
        When I query for bookings for a person using their NHS number
        Then the request is successful and no bookings are returned
