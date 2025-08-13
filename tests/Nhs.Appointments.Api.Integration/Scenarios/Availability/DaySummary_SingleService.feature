Feature: Get day summary for single service

  Scenario: Throws 501 when MultipleServices disabled
    Given the following sessions
      | Date        | From  | Until | Services                                  | Slot Length | Capacity |
      | Next Monday | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | Next Monday | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | Next Monday | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | Next Monday | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
    And the following bookings have been made
      | Date        | Time  | Duration | Service |
      | Next Monday | 09:20 | 10        | COVID   |
      | Next Monday | 09:20 | 10        | FLU     |
      | Next Monday | 09:20 | 10        | RSV     |
      | Next Monday | 09:20 | 10        | FLU-B   |
    When I query day summary for the current site on 'Next Monday'
    Then the call should fail with 501
