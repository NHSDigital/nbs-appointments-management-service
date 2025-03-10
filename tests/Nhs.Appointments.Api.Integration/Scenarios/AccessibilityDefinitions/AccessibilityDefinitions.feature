Feature: Get Accessibility Definitions

  Scenario: Retrieve list of accessibility definitions
  Given There are existing system accessibilities
    | Id                           | DisplayName   |
    | definition_one/accessibility_one | Accessibility One |
    | definition_one/accessibility_two | Accessibility Two |
    | definition_two/accessibility_one | Accessibility One |
    | definition_two/accessibility_two | Accessibility Two |
  When I query for all accessibility definitions
  Then the following accessibility definitions are returned
    | Id                           | DisplayName   |
    | definition_one/accessibility_one | Accessibility One |
    | definition_one/accessibility_two | Accessibility Two |
    | definition_two/accessibility_one | Accessibility One |
    | definition_two/accessibility_two | Accessibility Two |
