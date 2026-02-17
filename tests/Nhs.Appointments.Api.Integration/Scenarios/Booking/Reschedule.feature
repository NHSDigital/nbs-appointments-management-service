Feature: Appointment reschedule

    Scenario: Reschedule an appointment
        Given the following sessions exist for a created default site
            | Date     | From  | Until | Services | Slot Length | Capacity |
            | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
        And a citizen with the NHS Number '1234678891'
        And the following bookings have been made
            | Date     | Time  | Duration | Service |
            | Tomorrow | 09:20 | 5        | COVID   |
        When I make a provisional appointment with the following details
            | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        |
            | Tomorrow | 09:30 | 5        | COVID   | *          | Test      | One      | 2000-02-01 |
        And I confirm the rescheduled booking
        Then the rescheduled booking is no longer marked as provisional
        And the first booking has been 'Cancelled'
        And 'RescheduledByCitizen' cancellation reason has been used

    Scenario: Reschedule an appointment on a different day
      Given the following sessions exist for a created site '6e3348bf-3509-45f2-887c-4f9651501f04'
        | Date              | From  | Until | Services | Slot Length | Capacity |
        | Tomorrow          | 09:00 | 10:00 | COVID    | 5           | 1        |
        | 2 days from today | 09:00 | 12:00 | COVID    | 5           | 1        |
      And a citizen with the NHS Number '1234678892'
      And the following bookings have been made for site '6e3348bf-3509-45f2-887c-4f9651501f04'
        | Date     | Time  | Duration | Service |
        | Tomorrow | 09:20 | 5        | COVID   |
      #Wait for setup aggregation to be processed
      And an aggregation is created for site '6e3348bf-3509-45f2-887c-4f9651501f04', date 'Tomorrow', '0' cancelled bookings, and maximumCapacity '12', and with service details
        | Service  | Bookings    | Orphaned  | RemainingCapacity |
        | COVID    | 1           | 0         | 11                |
      And an aggregation is created for site '6e3348bf-3509-45f2-887c-4f9651501f04', date '2 days from today', '0' cancelled bookings, and maximumCapacity '36', and with service details
        | Service  | Bookings    | Orphaned  | RemainingCapacity |
        | COVID    | 0           | 0         | 36                |
      When I make a provisional appointment with the following details at site '6e3348bf-3509-45f2-887c-4f9651501f04'
        | Date              | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        |
        | 2 days from today | 10:30 | 5        | COVID   | *          | Test      | One      | 2000-02-01 |
      And I confirm the rescheduled booking
      Then the rescheduled booking is no longer marked as provisional at site '6e3348bf-3509-45f2-887c-4f9651501f04'
      And the first booking at site '6e3348bf-3509-45f2-887c-4f9651501f04' has been 'Cancelled'
      And 'RescheduledByCitizen' cancellation reason has been used at site '6e3348bf-3509-45f2-887c-4f9651501f04'
      And an aggregation updated recently for site '6e3348bf-3509-45f2-887c-4f9651501f04', date 'Tomorrow', '1' cancelled bookings, and maximumCapacity '12', and with service details
        | Service  | Bookings    | Orphaned  | RemainingCapacity |
        | COVID    | 0           | 0         | 12                |
      And an aggregation updated recently for site '6e3348bf-3509-45f2-887c-4f9651501f04', date '2 days from today', '0' cancelled bookings, and maximumCapacity '36', and with service details
        | Service  | Bookings    | Orphaned  | RemainingCapacity |
        | COVID    | 1           | 0         | 35                |
        
    Scenario: Cannot reschedule an appointment if the nhs number for both bookings does not match
        Given the following sessions exist for a created default site
            | Date     | From  | Until | Services | Slot Length | Capacity |
            | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
        And a citizen with the NHS Number '1234678891'
        And the following bookings have been made
            | Date     | Time  | Duration | Service |
            | Tomorrow | 09:20 | 5        | COVID   |
        When I make a provisional appointment with the following details
            | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        |
            | Tomorrow | 09:30 | 5        | COVID   | 9999999999 | Test      | One      | 2000-02-01 |
        And I confirm the rescheduled booking
        Then the call should fail with 412

    Scenario: Cannot reschedule appointment if appointment to reschedule is not found
        Given the following sessions exist for a created default site
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
        And a citizen with the NHS Number '1234678891'
        When I make a provisional appointment with the following details
            | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        |
            | Tomorrow | 09:30 | 5        | COVID   | 9999999999 | Test      | One      | 2000-02-01 |
        And I confirm the rescheduled booking
        Then the call should fail with 404
