Feature: Get week summary for multiple services

  Scenario: Returns Bad Request when fetched for a day not on a Monday
    Given the following sessions
      | Date          | From  | Until | Services                             | Slot Length | Capacity |
      | Next Thursday | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E | 10           | 1        |
      | Next Thursday | 09:00 | 10:00 | COVID, FLU-B                         | 10           | 1        |
      | Next Thursday | 09:00 | 10:00 | FLU, FLU-B, FLU-C                    | 10           | 1        |
      | Next Thursday | 09:00 | 10:00 | FLU-B, RSV                           | 10           | 1        |
    And the following bookings have been made
      | Date          | Time  | Duration  | Service |
      | Next Thursday | 09:20 | 10        | COVID   |
      | Next Thursday | 09:20 | 10        | FLU     |
      | Next Thursday | 09:20 | 10        | RSV     |
      | Next Thursday | 09:20 | 10        | FLU-B   |
    When I query week summary for the current site on 'Next Thursday'
    Then a bad request error is returned
  
  Scenario: Returns expected sessions based on service lengths
    Given the following sessions
      | Date            | From  | Until | Services                                  | Slot Length  | Capacity |
      | Next Monday     | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | Next Monday     | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | Next Monday     | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | Next Monday     | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | Next Tuesday    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | Next Tuesday    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | Next Tuesday    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | Next Tuesday    | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | Next Wednesday  | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | Next Wednesday  | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | Next Wednesday  | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | Next Wednesday  | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | Next Thursday   | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | Next Thursday   | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | Next Thursday   | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | Next Thursday   | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | Next Friday     | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | Next Friday     | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | Next Friday     | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | Next Friday     | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | Next Saturday   | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | Next Saturday   | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | Next Saturday   | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | Next Saturday   | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | Next Sunday     | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | Next Sunday     | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | Next Sunday     | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | Next Sunday     | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
    And the following bookings have been made
      | Date           | Time  | Duration  | Service |
      | Next Monday    | 09:20 | 10        | COVID   |
      | Next Monday    | 09:20 | 10        | FLU     |
      | Next Monday    | 09:20 | 10        | RSV     |
      | Next Monday    | 09:20 | 10        | FLU-B   |
      | Next Tuesday   | 09:20 | 10        | COVID   |
      | Next Tuesday   | 09:20 | 10        | FLU     |
      | Next Tuesday   | 09:20 | 10        | RSV     |
      | Next Tuesday   | 09:20 | 10        | FLU-B   |
      | Next Wednesday | 09:20 | 10        | COVID   |
      | Next Wednesday | 09:20 | 10        | FLU     |
      | Next Wednesday | 09:20 | 10        | RSV     |
      | Next Wednesday | 09:20 | 10        | FLU-B   |
      | Next Thursday  | 09:20 | 10        | COVID   |
      | Next Thursday  | 09:20 | 10        | FLU     |
      | Next Thursday  | 09:20 | 10        | RSV     |
      | Next Thursday  | 09:20 | 10        | FLU-B   |
      | Next Friday    | 09:20 | 10        | COVID   |
      | Next Friday    | 09:20 | 10        | FLU     |
      | Next Friday    | 09:20 | 10        | RSV     |
      | Next Friday    | 09:20 | 10        | FLU-B   |
      | Next Saturday  | 09:20 | 10        | COVID   |
      | Next Saturday  | 09:20 | 10        | FLU     |
      | Next Saturday  | 09:20 | 10        | RSV     |
      | Next Saturday  | 09:20 | 10        | FLU-B   |
      | Next Sunday    | 09:20 | 10        | COVID   |
      | Next Sunday    | 09:20 | 10        | FLU     |
      | Next Sunday    | 09:20 | 10        | RSV     |
      | Next Sunday    | 09:20 | 10        | FLU-B   |
    When I query week summary for the current site on 'Next Monday'
    Then the following week summary metrics are returned
      | Maximum Capacity | Remaining Capacity | Booked Appointments | Orphaned Appointments | Cancelled Appointments |
      | 168              | 147                | 28                  | 7                     | 0                      |
    And the following day summary metrics are returned
      | Date             | Maximum Capacity | Remaining Capacity | Booked Appointments | Orphaned Appointments | Cancelled Appointments |
      | Next Monday      | 24               | 21                 | 4                   | 1                     | 0                      |
      | Next Tuesday     | 24               | 21                 | 4                   | 1                     | 0                      |
      | Next Wednesday   | 24               | 21                 | 4                   | 1                     | 0                      |
      | Next Thursday    | 24               | 21                 | 4                   | 1                     | 0                      |
      | Next Friday      | 24               | 21                 | 4                   | 1                     | 0                      |
      | Next Saturday    | 24               | 21                 | 4                   | 1                     | 0                      |
      | Next Sunday      | 24               | 21                 | 4                   | 1                     | 0                      |
    And the following session summaries on day 'Next Monday' are returned
      | StartDate      | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | Next Monday    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | Next Monday    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | Next Monday    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | Next Monday    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day 'Next Tuesday' are returned
      | StartDate       | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | Next Tuesday    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | Next Tuesday    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | Next Tuesday    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | Next Tuesday    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day 'Next Wednesday' are returned
      | StartDate         | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | Next Wednesday    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | Next Wednesday    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | Next Wednesday    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | Next Wednesday    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day 'Next Thursday' are returned
      | StartDate        | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | Next Thursday    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | Next Thursday    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | Next Thursday    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | Next Thursday    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day 'Next Friday' are returned
      | StartDate      | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | Next Friday    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | Next Friday    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | Next Friday    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | Next Friday    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day 'Next Saturday' are returned
      | StartDate        | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | Next Saturday    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | Next Saturday    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | Next Saturday    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | Next Saturday    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day 'Next Sunday' are returned
      | StartDate      | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | Next Sunday    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | Next Sunday    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | Next Sunday    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | Next Sunday    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
