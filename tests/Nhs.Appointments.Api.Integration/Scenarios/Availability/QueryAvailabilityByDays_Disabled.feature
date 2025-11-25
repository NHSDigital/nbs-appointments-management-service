Feature: Query Availability By Days

  Scenario: Query Availability By Days
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I query availability by days
      | Site                                 | Service   | From     | Until             |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | RSV:Adult | Tomorrow | 2 days from today |
    Then the call should fail with 501
