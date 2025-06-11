Feature: Get week summary for multiple services

  Scenario: Returns expected sessions based on service lengths
    Given the following sessions
      | Date                 | From  | Until | Services                                  | Slot Length | Capacity |
      | Tomorrow             | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | Tomorrow             | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | Tomorrow             | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | Tomorrow             | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | 2 days from today    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | 2 days from today    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | 2 days from today    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | 2 days from today    | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | 3 days from today    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | 3 days from today    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | 3 days from today    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | 3 days from today    | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | 4 days from today    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | 4 days from today    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | 4 days from today    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | 4 days from today    | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | 5 days from today    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | 5 days from today    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | 5 days from today    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | 5 days from today    | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | 6 days from today    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | 6 days from today    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | 6 days from today    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | 6 days from today    | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | 7 days from today    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | 7 days from today    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | 7 days from today    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | 7 days from today    | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
    And the following bookings have been made
      | Date                 | Time  | Duration | Service |
      | Tomorrow             | 09:20 | 10        | COVID   |
      | Tomorrow             | 09:20 | 10        | FLU     |
      | Tomorrow             | 09:20 | 10        | RSV     |
      | Tomorrow             | 09:20 | 10        | FLU-B   |
      | 2 days from today    | 09:20 | 10        | COVID   |
      | 2 days from today    | 09:20 | 10        | FLU     |
      | 2 days from today    | 09:20 | 10        | RSV     |
      | 2 days from today    | 09:20 | 10        | FLU-B   |
      | 3 days from today    | 09:20 | 10        | COVID   |
      | 3 days from today    | 09:20 | 10        | FLU     |
      | 3 days from today    | 09:20 | 10        | RSV     |
      | 3 days from today    | 09:20 | 10        | FLU-B   |
      | 4 days from today    | 09:20 | 10        | COVID   |
      | 4 days from today    | 09:20 | 10        | FLU     |
      | 4 days from today    | 09:20 | 10        | RSV     |
      | 4 days from today    | 09:20 | 10        | FLU-B   |
      | 5 days from today    | 09:20 | 10        | COVID   |
      | 5 days from today    | 09:20 | 10        | FLU     |
      | 5 days from today    | 09:20 | 10        | RSV     |
      | 5 days from today    | 09:20 | 10        | FLU-B   |
      | 6 days from today    | 09:20 | 10        | COVID   |
      | 6 days from today    | 09:20 | 10        | FLU     |
      | 6 days from today    | 09:20 | 10        | RSV     |
      | 6 days from today    | 09:20 | 10        | FLU-B   |
      | 7 days from today    | 09:20 | 10        | COVID   |
      | 7 days from today    | 09:20 | 10        | FLU     |
      | 7 days from today    | 09:20 | 10        | RSV     |
      | 7 days from today    | 09:20 | 10        | FLU-B   |
    When I query week summary for the current site on 'Tomorrow'
    Then the following week summary metrics are returned
      | Maximum Capacity | Remaining Capacity | Booked Appointments | Orphaned Appointments | Cancelled Appointments |
      | 168              | 147                | 28                  | 7                     | 0                      |
    And the following day summary metrics are returned
      | Date               | Maximum Capacity | Remaining Capacity | Booked Appointments | Orphaned Appointments | Cancelled Appointments |
      | Tomorrow           | 24               | 21                 | 4                   | 1                     | 0                      |
      | 2 days from today  | 24               | 21                 | 4                   | 1                     | 0                      |
      | 3 days from today  | 24               | 21                 | 4                   | 1                     | 0                      |
      | 4 days from today  | 24               | 21                 | 4                   | 1                     | 0                      |
      | 5 days from today  | 24               | 21                 | 4                   | 1                     | 0                      |
      | 6 days from today  | 24               | 21                 | 4                   | 1                     | 0                      |
      | 7 days from today  | 24               | 21                 | 4                   | 1                     | 0                      |
    And the following session summaries on day 'Tomorrow' are returned
      | StartDate   | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | Tomorrow    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | Tomorrow    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | Tomorrow    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | Tomorrow    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day '2 days from today' are returned
      | StartDate            | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | 2 days from today    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | 2 days from today    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | 2 days from today    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | 2 days from today    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day '3 days from today' are returned
      | StartDate            | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | 3 days from today    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | 3 days from today    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | 3 days from today    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | 3 days from today    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day '4 days from today' are returned
      | StartDate            | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | 4 days from today    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | 4 days from today    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | 4 days from today    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | 4 days from today    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day '5 days from today' are returned
      | StartDate            | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | 5 days from today    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | 5 days from today    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | 5 days from today    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | 5 days from today    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day '6 days from today' are returned
      | StartDate            | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | 6 days from today    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | 6 days from today    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | 6 days from today    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | 6 days from today    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day '7 days from today' are returned
      | StartDate            | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | 7 days from today    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | 7 days from today    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | 7 days from today    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | 7 days from today    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    
