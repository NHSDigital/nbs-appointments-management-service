Feature: Get week summary for single service

  Scenario: Throws 501 when MultipleServices disabled
    Given the following sessions
      | Date        | From  | Until | Services                                  | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | Tomorrow    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | Tomorrow    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | Tomorrow    | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
    And the following bookings have been made
      | Date        | Time  | Duration | Service |
      | Tomorrow    | 09:20 | 10        | COVID   |
      | Tomorrow    | 09:20 | 10        | FLU     |
      | Tomorrow    | 09:20 | 10        | RSV     |
      | Tomorrow    | 09:20 | 10        | FLU-B   |
    When I query week summary for the current site on 'Tomorrow'
    Then the call should fail with 501
