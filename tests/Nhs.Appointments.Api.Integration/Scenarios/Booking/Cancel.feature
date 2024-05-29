Feature: Appointment cancellation

    Background: 
        Given The following service configuration
             | Code    | Duration |
             | COVID   | 5        |
             | FLU     | 8        |
             | COADMIN | 10       |
        And the following week templates
            | Name | Days | From  | Until | Services |
            | Test | All  | 09:00 | 10:00 | COVID    |
        And the following template assignments
            | Template | From       | Until      |
            | Test     | 2077-01-01 | 2077-01-31 |


    Scenario: Cancel a booking appointment
        Given The following bookings have been made
            | Date       | Time  | Duration | Service |
            | 2076-01-02 | 09:20 | 5        | COVID   |
        When I cancel the appointment 
        Then the appropriate booking has been 'Cancelled'
      