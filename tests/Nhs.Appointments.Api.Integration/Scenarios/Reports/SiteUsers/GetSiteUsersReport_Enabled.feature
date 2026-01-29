Feature: Download Site Users Report

  Scenario: Can download site users report when toggle is enabled
    Given the following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Type         | IsDeleted |
      | d8ce8d0a-6c95-421b-9136-9865fba555a1 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | 8d554c5a-329a-4454-a88d-d653484a9d42 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | be062313-e376-45f7-8394-920299e14679 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
      | e0f27adc-e5b2-4c4e-8488-39c87e046516 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  | false     |
    And the following role assignments for 'test-user-1' exist at site 'd8ce8d0a-6c95-421b-9136-9865fba555a1'
      | Roles                        |
      | canned:availability-manager  |
    And the following role assignments for 'test-user-2' exist at site '8d554c5a-329a-4454-a88d-d653484a9d42'
      | Roles                        |
      | canned:availability-manager  |
    And the following role assignments for 'test-user-3' exist at site 'be062313-e376-45f7-8394-920299e14679'
      | Roles                        |
      | canned:availability-manager  |
    And the following role assignments for 'test-user-4' exist at site 'e0f27adc-e5b2-4c4e-8488-39c87e046516'
      | Roles                        |
      | canned:availability-manager  |
    When I request a site users report
    Then the call should be successful
    And the report has the following headers
      | User |
    And the report contains the following data
      | User                                             |
      | test-user-1_d8ce8d0a-6c95-421b-9136-9865fba555a1 |
      | test-user-2_8d554c5a-329a-4454-a88d-d653484a9d42 |
      | test-user-3_be062313-e376-45f7-8394-920299e14679 |
      | test-user-4_e0f27adc-e5b2-4c4e-8488-39c87e046516 |
