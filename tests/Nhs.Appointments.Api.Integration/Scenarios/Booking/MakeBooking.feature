Feature: Book an appointment

    Scenario: Make a booking appointment
      Given the site is configured for MYA
      And the following sessions
        | Date       | From  | Until | Services | Slot Length | Capacity |
        | 2077-01-01 | 09:00 | 10:00 | COVID    | 5           | 1        |
      When I make the appointment for 'COVID' at '2077-01-01 09:20'
      Then the booking is created and the reference number is returned containing '000001'
              
    Scenario: Cannot book an appointment that is no longer available
      Given the following sessions
        | Date       | From  | Until | Services | Slot Length | Capacity |
        | 2077-01-01 | 09:00 | 10:00 | COVID    | 5           | 1        |
      And the following bookings have been made
        | Date       | Time  | Duration | Service |
        | 2077-01-01 | 09:45 | 5        | COVID   |
      When I make the appointment for 'COVID' at '2077-01-01 09:45'
      Then I receive a message informing me that the appointment is no longer available

    Scenario: Cannot book an appointment that is outside the session
      Given the following sessions
        | Date       | From  | Until | Services | Slot Length | Capacity |
        | 2077-01-01 | 09:00 | 10:00 | COVID    | 5           | 1        |
      When I make the appointment for 'COVID' at '2077-01-01 10:00'
      Then I receive a message informing me that the appointment is no longer available
      