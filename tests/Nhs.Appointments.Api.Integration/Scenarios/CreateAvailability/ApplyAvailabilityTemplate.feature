Feature: Create daily availability

  Scenario: Can apply an availability template
    Given there is no existing availability
    When I apply the following availability template
      | From         | Until        | Days                                                     | TimeFrom | TimeUntil | SlotLength | Capacity | Services |
      | Tomorrow_+_1 | Tomorrow_+_3 | Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday | 09:00    | 10:00     | 5          | 1        | COVID    |
    Then the request is successful and the following daily availability is created
      | Date         | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow_+_1 | 09:00 | 10:00 | COVID    | 5           | 1        |
      | Tomorrow_+_2 | 09:00 | 10:00 | COVID    | 5           | 1        |
      | Tomorrow_+_3 | 09:00 | 10:00 | COVID    | 5           | 1        |

  Scenario: Overwrites existing daily availability
    Given the following sessions
      | Date         | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow_+_1 | 09:00 | 17:00 | COVID    | 5           | 2        |
    When I apply the following availability template
      | From         | Until        | Days                                                     | TimeFrom | TimeUntil | SlotLength | Capacity | Services |
      | Tomorrow_+_1 | Tomorrow_+_1 | Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday | 11:00    | 12:00     | 5          | 1        | COVID    |
    Then the request is successful and the following daily availability is created
      | Date         | From  | Until | Services | Slot Length | Capacity |
      | Tomorrow_+_1 | 11:00 | 12:00 | COVID    | 5           | 1        |

    