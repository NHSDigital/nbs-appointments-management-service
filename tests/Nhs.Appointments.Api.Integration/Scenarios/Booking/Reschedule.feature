Feature: Appointment reschedule

    Scenario: Reschedule an appointment
        Given the site is configured for MYA
        And a citizen with the NHS Number '1234678891'
        And the following sessions
            | Date       | From  | Until | Services | Slot Length | Capacity |
            | 2076-01-02 | 09:00 | 10:00 | COVID    | 5           | 1        |
        And the following bookings have been made
            | Date       | Time  | Duration | Service |
            | 2076-01-02 | 09:20 | 5        | COVID   |
        When I make a provisional appointment with the following details
            | DateTime         | Duration | Service | NhsNumber  | FirstName | LastName | DOB        |
            | 2076-01-02 09:30 | 5        | COVID   | *          | Test      | One      | 2000-02-01 |
        And I confirm the rescheduled booking
        Then the rescheduled booking is no longer marked as provisional
        And  the original booking has been 'Cancelled'
