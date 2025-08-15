Feature: Get day summary
  
  Scenario: Returns expected sessions based on service lengths
    Given the following sessions
      | Date            | From  | Until | Services                                  | Slot Length  | Capacity |
      | Next Monday     | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | Next Monday     | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | Next Monday     | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | Next Monday     | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
    And the following bookings have been made
      | Date           | Time  | Duration  | Service |
      | Next Monday    | 09:20 | 10        | COVID   |
      | Next Monday    | 09:20 | 10        | FLU     |
      | Next Monday    | 09:20 | 10        | RSV     |
      | Next Monday    | 09:20 | 10        | FLU-B   |
    When I query day summary for the current site on 'Next Monday'
    Then the following day summary metrics are returned
      | Date             | Maximum Capacity | Remaining Capacity | Booked Appointments | Orphaned Appointments | Cancelled Appointments |
      | Next Monday      | 24               | 21                 | 3                   | 1                     | 0                      |
    And the following session summaries on day 'Next Monday' are returned
      | StartDate      | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | Next Monday    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | Next Monday    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | Next Monday    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | Next Monday    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
