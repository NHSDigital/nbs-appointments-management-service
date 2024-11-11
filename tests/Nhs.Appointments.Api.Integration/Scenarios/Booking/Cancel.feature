Feature: Appointment cancellation

    Scenario: Cancel a booking appointment
        Given the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
        And the following bookings have been made
          | Date     | Time  | Duration | Service |
          | Tomorrow | 09:20 | 5        | COVID   |
        When I cancel the appointment
        Then the booking has been 'Cancelled'