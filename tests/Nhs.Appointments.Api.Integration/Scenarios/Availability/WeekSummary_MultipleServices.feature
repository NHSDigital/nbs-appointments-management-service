Feature: Get week summary for multiple services

  Scenario: Returns expected sessions based on service lengths
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
    Then the following week summary metrics are returned
      | Maximum Capacity | Remaining Capacity | Booked Appointments | Orphaned Appointments | Cancelled Appointments |
      | 24               | 21                 | 4                   | 1                     | 0                      |
    And the following day summary metrics are returned
      | Date               | Maximum Capacity | Remaining Capacity | Booked Appointments | Orphaned Appointments | Cancelled Appointments |
      | Tomorrow           | 24               | 21                 | 4                   | 1                     | 0                      |
      | 2 days from today  | 0                | 0                  | 0                   | 0                     | 0                      |
      | 3 days from today  | 0                | 0                  | 0                   | 0                     | 0                      |
      | 4 days from today  | 0                | 0                  | 0                   | 0                     | 0                      |
      | 5 days from today  | 0                | 0                  | 0                   | 0                     | 0                      |
      | 6 days from today  | 0                | 0                  | 0                   | 0                     | 0                      |
      | 7 days from today  | 0                | 0                  | 0                   | 0                     | 0                      |
    And the following session summaries on day 'Tomorrow' are returned
      | StartDate   | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | Tomorrow    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | Tomorrow    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | Tomorrow    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | Tomorrow    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    
