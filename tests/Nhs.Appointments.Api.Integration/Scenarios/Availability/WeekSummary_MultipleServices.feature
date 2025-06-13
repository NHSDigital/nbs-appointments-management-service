Feature: Get week summary for multiple services

  Scenario: Returns Bad Request when fetched for a day not on a Monday
    Given the following sessions
      | Date                   | From  | Until | Services                                  | Slot Length | Capacity |
      | 12-06-2025             | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | 12-06-2025             | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | 12-06-2025             | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | 12-06-2025             | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
    And the following bookings have been made
      | Date                   | Time  | Duration | Service |
      | 12-06-2025             | 09:20 | 10        | COVID   |
      | 12-06-2025             | 09:20 | 10        | FLU     |
      | 12-06-2025             | 09:20 | 10        | RSV     |
      | 12-06-2025             | 09:20 | 10        | FLU-B   |
    When I query week summary for the current site on '12-06-2025'
    Then a bad request error is returned
  
  Scenario: Returns expected sessions based on service lengths
    Given the following sessions
      | Date                 | From  | Until | Services                                  | Slot Length | Capacity |
      | 16-06-2025             | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | 16-06-2025             | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | 16-06-2025             | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | 16-06-2025             | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | 17-06-2025    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | 17-06-2025    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | 17-06-2025    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | 17-06-2025    | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | 18-06-2025    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | 18-06-2025    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | 18-06-2025    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | 18-06-2025    | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | 19-06-2025    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | 19-06-2025    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | 19-06-2025    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | 19-06-2025    | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | 20-06-2025    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | 20-06-2025    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | 20-06-2025    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | 20-06-2025    | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | 21-06-2025    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | 21-06-2025    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | 21-06-2025    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | 21-06-2025    | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      | 22-06-2025    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
      | 22-06-2025    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      | 22-06-2025    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
      | 22-06-2025    | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
    And the following bookings have been made
      | Date          | Time  | Duration | Service |
      | 16-06-2025    | 09:20 | 10        | COVID   |
      | 16-06-2025    | 09:20 | 10        | FLU     |
      | 16-06-2025    | 09:20 | 10        | RSV     |
      | 16-06-2025    | 09:20 | 10        | FLU-B   |
      | 17-06-2025    | 09:20 | 10        | COVID   |
      | 17-06-2025    | 09:20 | 10        | FLU     |
      | 17-06-2025    | 09:20 | 10        | RSV     |
      | 17-06-2025    | 09:20 | 10        | FLU-B   |
      | 18-06-2025    | 09:20 | 10        | COVID   |
      | 18-06-2025    | 09:20 | 10        | FLU     |
      | 18-06-2025    | 09:20 | 10        | RSV     |
      | 18-06-2025    | 09:20 | 10        | FLU-B   |
      | 19-06-2025    | 09:20 | 10        | COVID   |
      | 19-06-2025    | 09:20 | 10        | FLU     |
      | 19-06-2025    | 09:20 | 10        | RSV     |
      | 19-06-2025    | 09:20 | 10        | FLU-B   |
      | 20-06-2025    | 09:20 | 10        | COVID   |
      | 20-06-2025    | 09:20 | 10        | FLU     |
      | 20-06-2025    | 09:20 | 10        | RSV     |
      | 20-06-2025    | 09:20 | 10        | FLU-B   |
      | 21-06-2025    | 09:20 | 10        | COVID   |
      | 21-06-2025    | 09:20 | 10        | FLU     |
      | 21-06-2025    | 09:20 | 10        | RSV     |
      | 21-06-2025    | 09:20 | 10        | FLU-B   |
      | 22-06-2025    | 09:20 | 10        | COVID   |
      | 22-06-2025    | 09:20 | 10        | FLU     |
      | 22-06-2025    | 09:20 | 10        | RSV     |
      | 22-06-2025    | 09:20 | 10        | FLU-B   |
    When I query week summary for the current site on '16-06-2025'
    Then the following week summary metrics are returned
      | Maximum Capacity | Remaining Capacity | Booked Appointments | Orphaned Appointments | Cancelled Appointments |
      | 168              | 147                | 28                  | 7                     | 0                      |
    And the following day summary metrics are returned
      | Date               | Maximum Capacity | Remaining Capacity | Booked Appointments | Orphaned Appointments | Cancelled Appointments |
      | 16-06-2025           | 24               | 21                 | 4                   | 1                     | 0                      |
      | 17-06-2025  | 24               | 21                 | 4                   | 1                     | 0                      |
      | 18-06-2025  | 24               | 21                 | 4                   | 1                     | 0                      |
      | 19-06-2025  | 24               | 21                 | 4                   | 1                     | 0                      |
      | 20-06-2025  | 24               | 21                 | 4                   | 1                     | 0                      |
      | 21-06-2025  | 24               | 21                 | 4                   | 1                     | 0                      |
      | 22-06-2025  | 24               | 21                 | 4                   | 1                     | 0                      |
    And the following session summaries on day '16-06-2025' are returned
      | StartDate   | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | 16-06-2025    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | 16-06-2025    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | 16-06-2025    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | 16-06-2025    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day '17-06-2025' are returned
      | StartDate            | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | 17-06-2025    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | 17-06-2025    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | 17-06-2025    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | 17-06-2025    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day '18-06-2025' are returned
      | StartDate            | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | 18-06-2025    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | 18-06-2025    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | 18-06-2025    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | 18-06-2025    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day '19-06-2025' are returned
      | StartDate            | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | 19-06-2025    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | 19-06-2025    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | 19-06-2025    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | 19-06-2025    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day '20-06-2025' are returned
      | StartDate            | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | 20-06-2025    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | 20-06-2025    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | 20-06-2025    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | 20-06-2025    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day '21-06-2025' are returned
      | StartDate            | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | 21-06-2025    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | 21-06-2025    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | 21-06-2025    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | 21-06-2025    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
    And the following session summaries on day '22-06-2025' are returned
      | StartDate            | Start Time | End Time | Bookings                                          | Capacity | Slot Length | Maximum Capacity |
      | 22-06-2025    | 09:00      | 10:00    | COVID:0, RSV:0, FLU:0, FLU-C:0, FLU-D:0, FLU-E:0  | 1        | 10          | 6                |
      | 22-06-2025    | 09:00      | 10:00    | COVID:1, FLU-B:0                                  | 1        | 10          | 6                |
      | 22-06-2025    | 09:00      | 10:00    | FLU:1, FLU-B:0, FLU-C:0                           | 1        | 10          | 6                |
      | 22-06-2025    | 09:00      | 10:00    | FLU-B:0, RSV:1                                    | 1        | 10          | 6                |
