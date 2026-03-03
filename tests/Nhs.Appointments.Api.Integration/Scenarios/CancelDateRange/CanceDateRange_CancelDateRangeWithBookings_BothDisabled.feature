Feature: Cancel a date range

  Scenario: Cancel a date range both cancel date range and cancel date range with bookings disabled
    Given the following default site exists in the system
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I cancel sessions and bookings for the default site within a date range
      | From     | To                | CancelBookings |
      | Tomorrow | 3 days from today | false          |
    Then the call should fail with 501
