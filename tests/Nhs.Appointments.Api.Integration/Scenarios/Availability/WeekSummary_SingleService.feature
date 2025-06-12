Feature: Get week summary for single service

  Scenario: Throws 501 when MultipleServices disabled
    Given the following sessions
      | Date          | From  | Until | Services                                  | Slot Length | Capacity |
      | 16-06-2025    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | 16-06-2025    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | 16-06-2025    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | 16-06-2025    | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
    And the following bookings have been made
      | Date          | Time  | Duration | Service |
      | 16-06-2025    | 09:20 | 10        | COVID   |
      | 16-06-2025    | 09:20 | 10        | FLU     |
      | 16-06-2025    | 09:20 | 10        | RSV     |
      | 16-06-2025    | 09:20 | 10        | FLU-B   |
    When I query week summary for the current site on '16-06-2025'
    Then the call should fail with 501
