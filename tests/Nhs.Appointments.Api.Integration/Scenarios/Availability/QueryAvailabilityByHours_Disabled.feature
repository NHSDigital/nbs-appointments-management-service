Feature: Query Availability By Hours

  Scenario: Query Availability By Hours
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    When I query availability by hours
      | Site                                 | Attendee Services   | From     | Until             |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | RSV:Adult           | Tomorrow | 2 days from today |
    Then the call should fail with 501
