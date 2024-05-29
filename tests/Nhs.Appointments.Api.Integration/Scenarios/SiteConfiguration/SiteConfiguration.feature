Feature: Site Configuration

  Background:
    Given The following service configuration
      | Code          | Duration |
      | COVID:5_11    | 5        |
      | COVID:12_15   | 5        |
      | COVID:16_17   | 5        |
      | COVID:18_74   | 5        |
    
  Scenario: Setting a site configurations service to disabled
    Given The following week template
      | Name | Days      | From  | Until | Services                            |
      | Test | Monday    | 09:00 | 17:00 | COVID:12_15,COVID:16_17,COVID:18_74 |
      | Test | Wednesday | 09:00 | 17:00 | COVID:18_74                         |
    When service type 'COVID:18_74' is disabled
    Then the following week templates exist
      | Days    | From  | Until | Services                |
      | Monday  | 09:00 | 17:00 | COVID:12_15,COVID:16_17 |