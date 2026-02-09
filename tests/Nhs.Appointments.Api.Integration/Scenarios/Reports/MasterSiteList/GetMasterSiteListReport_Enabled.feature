Feature: Download Master Site List Report

  Scenario: Can download master site list report when toggle is enabled
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Type           | IsDeleted | Status  |
      | d8ce8d0a-6c95-421b-9136-9865fba555a1 | Site-1 | 1 Roadside | 0113 1111111 | J11     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice 1  | false     | Online  |
      | 8d554c5a-329a-4454-a88d-d653484a9d42 | Site-2 | 2 Roadside | 0113 2222222 | J12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice 2  | true      | Offline |
    When I request master site list report
    Then the call should be successful
    And the report has the following headers
      | Site Name | ODS Code | Site Type | Region | ICB | GUID | IsDeleted | Status | Long | Lat | Address |
    And the report contains the following data
      | Site Name | ODS Code | Site Type     | Region | ICB  | GUID                                 | IsDeleted | Status  | Long        | Lat       | Address    |   
      | Site-1    | J11      | GP Practice 1 | R1     | ICB1 | d8ce8d0a-6c95-421b-9136-9865fba555a1 | False     | Online  | 0.082750916 | 51.494056 | 1 Roadside |
      | Site-2    | J12      | GP Practice 2 | R2     | ICB2 | 8d554c5a-329a-4454-a88d-d653484a9d42 | True      | Offline | 0.082750916 | 51.494056 | 2 Roadside |

