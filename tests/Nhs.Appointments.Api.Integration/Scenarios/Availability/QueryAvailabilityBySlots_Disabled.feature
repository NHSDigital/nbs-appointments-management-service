Feature: Query Availability By Slots

  Scenario: Query Availability By Slots
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I query availability by slots
      | Site                                 | Attendee Services   | Date     | From  | Until |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | RSV:Adult           | Tomorrow | 09:00 | 17:00 |
    Then the call should fail with 501
