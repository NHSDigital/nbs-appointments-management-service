Feature: Appointment reschedule

    Scenario: Reschedule an appointment
        Given the site is configured for MYA
        And a citizen with the NHS Number '1234678891'
        And the following sessions
            | Date     | From  | Until | Services | Slot Length | Capacity |
            | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
        And the following bookings have been made
            | Date     | Time  | Duration | Service |
            | Tomorrow | 09:20 | 5        | COVID   |
        When I make a provisional appointment with the following details
            | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        |
            | Tomorrow | 09:30 | 5        | COVID   | *          | Test      | One      | 2000-02-01 |
        And I confirm the rescheduled booking
        Then the rescheduled booking is no longer marked as provisional
        And  the original booking has been 'Cancelled'
        
    Scenario: Cannot reschedule an appointment if the nhs number for both bookings does not match
        Given the site is configured for MYA
        And a citizen with the NHS Number '1234678891'
        And the following sessions
            | Date     | From  | Until | Services | Slot Length | Capacity |
            | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
        And the following bookings have been made
            | Date     | Time  | Duration | Service |
            | Tomorrow | 09:20 | 5        | COVID   |
        When I make a provisional appointment with the following details
            | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        |
            | Tomorrow | 09:30 | 5        | COVID   | 9999999999 | Test      | One      | 2000-02-01 |
        And I confirm the rescheduled booking
        Then the call should fail with 412

    Scenario: Cannot reschedule appointment if appointment to reschedule is not found
        Given the site is configured for MYA
        And a citizen with the NHS Number '1234678891'
        And the following sessions
            | Date     | From  | Until | Services | Slot Length | Capacity |
            | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
        When I make a provisional appointment with the following details
            | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        |
            | Tomorrow | 09:30 | 5        | COVID   | 9999999999 | Test      | One      | 2000-02-01 |
        And I confirm the rescheduled booking
        Then the call should fail with 404