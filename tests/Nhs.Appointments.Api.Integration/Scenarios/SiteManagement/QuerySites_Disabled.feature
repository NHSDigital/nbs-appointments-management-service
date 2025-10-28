Feature: Query Sites

  Scenario: Query Sites By Type and ODS Code
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
      | 2686db3e-18df-416d-82f0-b758fc0d92dd | Site-B | 1B Site Lane | 0113 1111112 | 20N     | R2     | ICB2 | Info 2                 | accessibility/attr_one=true  | -60       | -60      | Pharmacy    |
      | a010ae04-98b5-4df5-aa57-9c27673ca854 | Site-C | 1C Site Lane | 0113 1111113 | 25N     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | -60       | -60      | GP Practice |
    When I query sites by site type and ODS code
      | Types    | OdsCode | Longitude | Latitude | SearchRadius |
      | Pharmacy | 20N     | -60       | -60      | 10000        |
    Then the call should fail with 501
