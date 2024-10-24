﻿Feature: Create daily availability

  Scenario: Can apply an availability template
    Given there is no existing availability
    When I apply the following availability template
      | From       | Until      | Days            | TimeFrom | TimeUntil | SlotLength | Capacity | Services |
      | 2077-01-01 | 2077-01-03 | Friday,Saturday | 09:00    | 10:00     | 5          | 1        | COVID    |
    Then the request is successful and the following daily availability is created
      | Date       | From  | Until | Services | Slot Length | Capacity |
      | 2077-01-01 | 09:00 | 10:00 | COVID    | 5           | 1        |
      | 2077-01-02 | 09:00 | 10:00 | COVID    | 5           | 1        |

  Scenario: Overwrites existing daily availability 
    Given the following sessions
      | Date       | From  | Until | Services | Slot Length | Capacity |
      | 2077-01-01 | 09:00 | 17:00 | COVID    | 5           | 2        |
    When I apply the following availability template
      | From       | Until      | Days   | TimeFrom | TimeUntil | SlotLength | Capacity | Services |
      | 2077-01-01 | 2077-01-01 | Friday | 11:00    | 12:00     | 5          | 1        | COVID    |
    Then the request is successful and the following daily availability is created
      | Date       | From  | Until | Services | Slot Length | Capacity |
      | 2077-01-01 | 11:00 | 12:00 | COVID    | 5           | 1        |

    