Feature: Set availability

    Scenario: Make a booking appointment
      Given the site is configured for MYA
      When I apply the following availability
      | Date       | From  | Until | SlotLength | Capacity | Services |
      | 2024-10-10 | 09:00 | 17:00 | 5          | 1        | COVID    |
      Then the request is successful and the following availability is created
      | Date       | From  | Until | Services | Slot Length | Capacity |
      | 2024-10-10 | 09:00 | 17:00 | COVID    | 5           | 1        |