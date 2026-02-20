Feature: Query Availability By Slots

  Scenario: Query Availability By Slots
    Given the following sites exist in the system
      | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I query availability by slots at the default site
      | Attendee Services   | Date     | From  | Until |
      | RSV:Adult           | Tomorrow | 09:00 | 17:00 |
    Then the call should fail with 501
