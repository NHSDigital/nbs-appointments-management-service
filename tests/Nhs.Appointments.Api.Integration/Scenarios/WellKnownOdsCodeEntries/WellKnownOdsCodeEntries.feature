Feature: Get Well Known ODS Code Entries

  Scenario: Retrieve list of well known ods code entries
  Given there are existing well known ods codes documented
  When I query for well known ods code entries
  Then the following entries are returned
    | OdsCode | DisplayName               | Type   |
    | R1      | Region One                | region |
    | ICB1    | Integrated Care Board One | icb    |
