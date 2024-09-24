Feature: Get Attribute Definitions

  Scenario: Retrieve list of attribute definitions
  Given There are existing system attributes
    | Id                           | DisplayName   |
    | definition_one/attribute_one | Attribute One |
    | definition_one/attribute_two | Attribute Two |
    | definition_two/attribute_one | Attribute One |
    | definition_two/attribute_two | Attribute Two |
  When I query for all attribute definitions
  Then the following attribute definitions are returned
    | Id                           | DisplayName   |
    | definition_one/attribute_one | Attribute One |
    | definition_one/attribute_two | Attribute Two |
    | definition_two/attribute_one | Attribute One |
    | definition_two/attribute_two | Attribute Two |