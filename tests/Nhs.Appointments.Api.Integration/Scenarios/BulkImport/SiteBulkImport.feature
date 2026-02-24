Feature: Site Bulk Import

  Scenario: Import Sites
    Given the default site exists
    When I import the following sites
      | OdsCode | Name      | Address     | PhoneNumber | Longitude | Latitude   | ICB  | Region | Site type | accessible_toilet | braille_translation_service | disabled_car_parking | car_parking | induction_loop | sign_language_service | step_free_access | text_relay | wheelchair_access | Id                                   |
      | R1      | Test Site | 1 Test Lane | N           | -6.3125   | -55.741702 | ICB1 | R1     | Pharmacy  | TRUE              | FALSE                       | FALSE                | TRUE        | FALSE          | FALSE                 | TRUE             | FALSE      | FALSE             | a4f75fb5-73ba-4296-a7d1-afdc34260dc4 |
    Then I receive a report that the import was successful

  Scenario: Import site and update existing site
    Given the following sites with valid Guid IDs exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                | Longitude   | Latitude  | Type         | IsDeleted |
      | cc4274bc-7e0c-42f4-b412-b0f53f0e8d4c | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/car_parking=true | -60         | -60       | GP Practice  | false     |
    When I import the following sites
      | OdsCode | Name         | Address     | PhoneNumber  | Longitude   | Latitude  | ICB  | Region | Site type   | accessible_toilet | braille_translation_service | disabled_car_parking | car_parking | induction_loop | sign_language_service | step_free_access | text_relay | wheelchair_access | Id                                   |
      | R1      | Updated site | 1 Test Lane | 0113 1111112 | -61         | -62       | ICB2 | R1     | PCN Site    | FALSE             | FALSE                       | TRUE                 | FALSE       | FALSE          | FALSE                 | FALSE            | FALSE      | FALSE             | cc4274bc-7e0c-42f4-b412-b0f53f0e8d4c |
      | R2      | Test Site 2  | 2 Test Road | N            | -6.3125     | -55.74170 | ICB2 | R2     | GP Practice | FALSE             | FALSE                       | FALSE                | TRUE        | FALSE          | FALSE                 | FALSE            | FALSE      | FALSE             | 8e9006ea-a5b1-4bef-b762-bc4a496c9dec |
    Then I receive a report that the import was successful
    # Verify updated site
    When I request site information for site 'cc4274bc-7e0c-42f4-b412-b0f53f0e8d4c'
    Then the correct site is returned
      | Site                                 | Name         | Address     | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                                                                                                                                                                                                                                                                                                      | Longitude   | Latitude  | Type     |
      | cc4274bc-7e0c-42f4-b412-b0f53f0e8d4c | Updated site | 1 Test Lane | 0113 1111112 | R1      | R1     | ICB2 | Info 1                 | accessibility/accessible_toilet=false,accessibility/braille_translation_service=false,accessibility/disabled_car_parking=true,accessibility/car_parking=false,accessibility/induction_loop=false,accessibility/sign_language_service=false,accessibility/step_free_access=false,accessibility/text_relay=false,accessibility/wheelchair_access=false | -60         | -60       | PCN Site |
    # Verify new site
    When I request site information for site '8e9006ea-a5b1-4bef-b762-bc4a496c9dec'
    Then the correct site is returned
      | Site                                 | Name        | Address     | PhoneNumber | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                                                                                                                                                                                                                                                                                                                      | Longitude | Latitude   | Type        |
      | 8e9006ea-a5b1-4bef-b762-bc4a496c9dec | Test Site 2 | 2 Test Road | N           | R2      | R2     | ICB2 |                        | accessibility/accessible_toilet=false,accessibility/braille_translation_service=false,accessibility/disabled_car_parking=false,accessibility/car_parking=true,accessibility/induction_loop=false,accessibility/sign_language_service=false,accessibility/step_free_access=false,accessibility/text_relay=false,accessibility/wheelchair_access=false | -6.3125   | -55.741702 | GP Practice |
