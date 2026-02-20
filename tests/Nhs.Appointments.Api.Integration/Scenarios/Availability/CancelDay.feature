Feature: Cancel All Availability And Bookings On A Given Day

    Scenario: Dates and sessions are returned within date range
        Given the following sessions exist for a created default site
          | Date              | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow          | 09:00 | 17:00 | COVID    | 5           | 1        |
        And the following bookings have been made at the default site
          | Date     | Time  | Duration | Service | Reference   |
          | Tomorrow | 09:20 | 5        | COVID   | 12345-54321 |
        When I cancel the day 'Tomorrow' at the default site
        Then the booking at the default site with reference '12345-54321' has been 'Cancelled'
        And there are no sessions for 'Tomorrow' at the default site
