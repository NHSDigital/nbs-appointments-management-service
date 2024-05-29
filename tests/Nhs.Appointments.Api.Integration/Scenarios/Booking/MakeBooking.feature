Feature: Book an appointment

    Background: 
        Given The following service configuration for site 'A'
             | Code    | Duration |
             | COVID   | 5        |
             | FLU     | 8        |
             | COADMIN | 10       |
          And the following service configuration for site 'B'
            | Code    | Duration |
            | COVID   | 5        |
            | FLU     | 8        |
            | COADMIN | 10       |

    Scenario: Make a booking appointment
      Given The following week templates
        | Name | Days | From  | Until | Services |
        | Test | All  | 09:00 | 10:00 | COVID    |
      And the following template assignments
        | Template | From       | Until      |
        | Test     | 2077-01-01 | 2077-01-31 |
      When I make the appointment for 'COVID' at '2077-01-01 09:20'
      Then the booking is created and the reference number is returned containing '000001'
        
    Scenario: Booking reference refers to number of bookings at chosen site   
        Given The following week templates for site 'A'
          | Name | Days | From  | Until | Services |
          | Test | All  | 09:00 | 10:00 | COVID    |
        And the following template assignments for site 'A'
          | Template | From       | Until      |
          | Test     | 2076-01-01 | 2077-01-31 |
        And the following week templates for site 'B'
          | Name | Days | From  | Until | Services |
          | Test | All  | 09:00 | 10:00 | COVID    |
        And the following template assignments for site 'B'
          | Template | From       | Until      |
          | Test     | 2076-01-01 | 2077-01-31 |
        And the following bookings have been made for site 'A'
          | Date       | Time  | Duration | Service |
          | 2076-01-02 | 09:20 | 5        | COVID   |
          | 2076-01-02 | 09:25 | 5        | COVID   |
        When I make the appointment at site 'B' for 'COVID' at '2077-01-01 09:20'
        Then the booking is created and the reference number is returned containing '000001'
      
    Scenario: Cannot book an appointment that is no longer available
      Given The following week templates
        | Name | Days | From  | Until | Services |
        | Test | All  | 09:00 | 10:00 | COVID    |
      And the following template assignments
        | Template | From       | Until      |
        | Test     | 2077-01-01 | 2077-01-31 |
      And the following bookings have been made
        | Date       | Time  | Duration | Service |
        | 2077-01-01 | 09:45 | 5        | COVID   |
      When I make the appointment for 'COVID' at '2077-01-01 09:45'
      Then I receive a message informing me that the appointment is no longer available

    Scenario: Cannot book an appointment that is outside the session template
      Given The following week templates
        | Name | Days | From  | Until | Services |
        | Test | All  | 09:00 | 10:00 | COVID    |
      And the following template assignments
        | Template | From       | Until      |
        | Test     | 2077-01-01 | 2077-01-31 |
      When I make the appointment for 'COVID' at '2077-01-01 10:00'
      Then I receive a message informing me that the appointment is no longer available
      