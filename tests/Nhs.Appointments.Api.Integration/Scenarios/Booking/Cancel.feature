Feature: Appointment cancellation

    Scenario: Cancel a booking appointment
        Given the following sessions
            | Date       | From  | Until | Services | Slot Length | Capacity |
            | 2076-01-02 | 09:00 | 10:00 | COVID    | 5           | 1        |
        And the following bookings have been made
            | Date       | Time  | Duration | Service |
            | 2076-01-02 | 09:20 | 5        | COVID   |
        When I cancel the appointment 
        Then the appropriate booking has been 'Cancelled'
      