Feature: Get all bookings for a person using NHS Number

    Background: 
        Given The following service configuration
             | Code    | Duration |
             | COVID   | 5        |
             | FLU     | 8        |
        And the following week templates
            | Name | Days | From  | Until | Services   |
            | Test | All  | 09:00 | 10:00 | COVID, FLU |
        And the following template assignments
            | Template | From       | Until      |    
            | Test     | 2077-01-01 | 2077-01-31 |

    Scenario: Get all bookings for a patient
        Given The following bookings have been made
            | Date       | Time  | Duration | Service |
            | 2077-01-01 | 09:00 | 5        | COVID   |
            | 2077-01-02 | 09:15 | 8        | FLU     |
        When I query for bookings for a person using their NHS number 
        Then the following bookings are returned
          | Date       | Time  | Duration | Service |
          | 2077-01-01 | 09:00 | 5        | COVID   |
          | 2077-01-02 | 09:15 | 8        | FLU     |

    Scenario: Returns success if no bookings are found for a person 
        When I query for bookings for a person using their NHS number
        Then the request is successful and no bookings are returned
