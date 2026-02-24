Feature: Appointment reschedule

    Scenario: Reschedule an appointment
        Given the following sessions exist for a created default site
            | Date     | From  | Until | Services | Slot Length | Capacity |
            | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
        And a citizen with the NHS Number '1234678891'
        And the following bookings have been made at the default site
            | Date     | Time  | Duration | Service |
            | Tomorrow | 09:20 | 5        | COVID   |
        When I make a provisional appointment with the following details at the default site
            | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        |
            | Tomorrow | 09:30 | 5        | COVID   | *          | Test      | One      | 2000-02-01 |
        And I extract the rescheduled booking reference
        And I confirm the rescheduled booking
        Then the rescheduled booking is no longer marked as provisional at the default site
        And the first booking at the default site has been 'Cancelled'
        And 'RescheduledByCitizen' cancellation reason has been used for the first booking at the default site

    Scenario: Reschedule an appointment on a different day - aggregation
      Given I set a single siteId for the test to be '6e3348bf-3509-45f2-887c-4f9651501f04'
      And the following sessions exist for a created default site
        | Date              | From  | Until | Services | Slot Length | Capacity |
        | Tomorrow          | 09:00 | 10:00 | COVID    | 5           | 1        |
        | 2 days from today | 09:00 | 12:00 | COVID    | 5           | 1        |
      And a citizen with the NHS Number '1234678892'
      And the following bookings have been made at the default site
        | Date     | Time  | Duration | Service |
        | Tomorrow | 09:20 | 5        | COVID   |
      #Wait for setup aggregation to be processed
      And an aggregation is created for the default site for 'Tomorrow' with '0' cancelled bookings, maximumCapacity '12', and with service details
        | Service      | Bookings    | Orphaned  | RemainingCapacity |
        | COVID        | 1           | 0         | 11                |
      And an aggregation is created for the default site for '2 days from today' with '0' cancelled bookings, maximumCapacity '36', and with service details
        | Service      | Bookings    | Orphaned  | RemainingCapacity |
        | COVID        | 0           | 0         | 36                |
      When I make a provisional appointment with the following details at the default site
        | Date              | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        |
        | 2 days from today | 10:30 | 5        | COVID   | *          | Test      | One      | 2000-02-01 |
      And I extract the rescheduled booking reference
      And I confirm the rescheduled booking
      Then the rescheduled booking is no longer marked as provisional at the default site
      And the first booking at the default site has been 'Cancelled'
      And 'RescheduledByCitizen' cancellation reason has been used for the first booking at the default site
      And an aggregation for the default site updated recently for 'Tomorrow' with '1' cancelled bookings, maximumCapacity '12', and with service details
        | Service      | Bookings    | Orphaned  | RemainingCapacity |
        | COVID        | 0           | 0         | 12                |
      And an aggregation for the default site updated recently for '2 days from today' with '0' cancelled bookings, maximumCapacity '36', and with service details
        | Service      | Bookings    | Orphaned  | RemainingCapacity |
        | COVID        | 1           | 0         | 35                |

  Scenario: Reschedule an appointment on a different day - audit trail
    Given I set a single siteId for the test to be '562348bf-3509-45f2-887c-4f9651501f06'
    And the following sessions exist for a created default site
      | Date              | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 2 days from today | 09:00 | 12:00 | COVID    | 5           | 1        |
    And a citizen with the NHS Number '1234678892'
    And the following bookings have been made at the default site
      | Date     | Time  | Duration | Service |
      | Tomorrow | 09:20 | 5        | COVID   |
#   Audit setup assertion
    And the original booking with details has a sanitized audit and index in blob storage at the default site
      | Appointment Status | Availability Status | Last Updated By |
      | Booked             | Supported           | api@test        |
#   Action 1
    When I make a provisional appointment with the following details at the default site
      | Date              | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        |
      | 2 days from today | 10:30 | 5        | COVID   | *          | Test      | One      | 2000-02-01 |
    And I extract the rescheduled booking reference
#   Audit assertion 1
    And the rescheduled booking with details has a sanitized audit and index in blob storage at the default site
      | Appointment Status | Availability Status | Last Updated By |
      | Provisional        | Supported           | api@test        |
#   Action 2
    When I confirm the rescheduled booking
    Then the rescheduled booking is no longer marked as provisional at the default site
#   Audit assertion 2
    And the rescheduled booking with details has a sanitized audit and index in blob storage at the default site
      | Appointment Status | Availability Status | Last Updated By |
      | Booked             | Supported           | api@test        |
    And the first booking at the default site has been 'Cancelled'
    And 'RescheduledByCitizen' cancellation reason has been used for the first booking at the default site
#   Audit assertion 3
    And the original booking with details has a sanitized audit and index in blob storage at the default site
      | Appointment Status | Availability Status | Last Updated By |
      | Cancelled          | Unknown             | api@test        |
        
    Scenario: Cannot reschedule an appointment if the nhs number for both bookings does not match
        Given the following sessions exist for a created default site
            | Date     | From  | Until | Services | Slot Length | Capacity |
            | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
        And a citizen with the NHS Number '1234678891'
        And the following bookings have been made at the default site
            | Date     | Time  | Duration | Service |
            | Tomorrow | 09:20 | 5        | COVID   |
        When I make a provisional appointment with the following details at the default site
            | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        |
            | Tomorrow | 09:30 | 5        | COVID   | 9999999999 | Test      | One      | 2000-02-01 |
        And I extract the rescheduled booking reference
        And I confirm the rescheduled booking
        Then the call should fail with 412

    Scenario: Cannot reschedule appointment if appointment to reschedule is not found
        Given the following sessions exist for a created default site
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
        And a citizen with the NHS Number '1234678891'
        When I make a provisional appointment with the following details at the default site
            | Date     | Time  | Duration | Service | NhsNumber  | FirstName | LastName | DOB        |
            | Tomorrow | 09:30 | 5        | COVID   | 9999999999 | Test      | One      | 2000-02-01 |
        And I extract the rescheduled booking reference
        And I confirm the rescheduled booking
        Then the call should fail with 404
