Feature: Site Bulk Import

  Scenario: Import Sites
    Given the site is configured for MYA
    When I import the following site
      | OdsCode | Name      | Address     | PhoneNumber | Longitude | Latitude  | ICB  | Region | Site type | accessible_toilet | braille_translation_service | disabled_car_parking | car_parking | induction_loop | sign_language_service | step_free_access | text_relay | wheelchair_access |
      | ODS1    | Test Site | 1 Test Lane | N           | -6.3125   | 55.741702 | ICB1 | R1     | Pharmacy  | TRUE              | FALSE                       | FALSE                | TRUE        | FALSE          | FALSE                 | TRUE             | FALSE      | FALSE             |
    Then the call should fail with 501
