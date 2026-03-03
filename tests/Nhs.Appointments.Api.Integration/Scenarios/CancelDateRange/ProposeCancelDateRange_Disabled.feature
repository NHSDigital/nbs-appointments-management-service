Feature: Propose cancel a date range

  Scenario: Propose cancel a date range
    Given the following default site exists in the system
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I propose cancelling sessions and bookings for the default site within a date range
      | From     | To                |
      | Tomorrow | 3 days from today |
    Then the call should fail with 501
