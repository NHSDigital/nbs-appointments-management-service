Feature: Get daily availability for multiple service scenarios

    Scenario: Dates and availability are only returned from sessions for that service
        Given the following sessions
          | Date        | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow    | 09:00 | 17:00 | RSV    | 5           | 1        |
          | 2 days from today | 09:00 | 17:00 | RSV, COVID    | 5           | 1        |
          | 3 days from today | 09:00 | 17:00 | COVID    | 5           | 1        |
          | 4 days from today | 09:00 | 17:00 | COVID, FLU    | 5           | 1        |
        When I check daily availability for 'COVID' between 'Tomorrow' and '4 days from today'
        Then the following daily availability is returned
          | Date        | Am | Pm |
          | Tomorrow    | 0 | 0 |
          | 2 days from today | 36 | 60 |
          | 3 days from today | 36 | 60 |
          | 4 days from today | 36 | 60 |

    Scenario: Dates and availability are returned from overlapping sessions of different services
        Given the following sessions
          | Date     | From  | Until | Services    | Slot Length  | Capacity |
          | Tomorrow | 09:00 | 10:00 | RSV, COVID  | 5            | 3        |
          | Tomorrow | 09:00 | 10:00 | COVID       | 5            | 2        |
          | Tomorrow | 09:00 | 10:00 | RSV         | 5            | 3        |
        When I check daily availability for 'COVID' between 'Tomorrow' and '3 days from today'
        Then the following daily availability is returned
          | Date        | Am | Pm |
          | Tomorrow    | 60 | 0  |
          | 2 days from today | 0  | 0  |
          | 3 days from today | 0  | 0  |

    Scenario: Bookings of other services take up availability
        Given the following sessions
          | Date        | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow    | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
          | 2 days from today | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
          | 3 days from today | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
        And the following bookings have been made
          | Date        | Time  | Duration | Service |
          | 2 days from today | 09:20 | 5        | FLU   |
        When I check daily availability for 'COVID' between 'Tomorrow' and '3 days from today'
        Then the following daily availability is returned
          | Date        | Am | Pm |
          | Tomorrow    | 12 | 0  |
          | 2 days from today | 11 | 0  |
          | 3 days from today | 12 | 0  |
      
    Scenario: Provisional bookings of other services take up availability
        Given the following sessions
          | Date        | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow    | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
          | 2 days from today | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
          | 3 days from today | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
        And the following provisional bookings have been made
          | Date        | Time  | Duration | Service |
          | 2 days from today | 09:20 | 5        | FLU   |
        When I check daily availability for 'COVID' between 'Tomorrow' and '3 days from today'
        Then the following daily availability is returned
          | Date        | Am | Pm |
          | Tomorrow    | 12 | 0  |
          | 2 days from today | 11 | 0  |
          | 3 days from today | 12 | 0  |

    Scenario: Expired provisional bookings of other services don't take up availability
        Given the following sessions
          | Date        | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow    | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
          | 2 days from today | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
          | 3 days from today | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
        And the following expired provisional bookings have been made
          | Date        | Time  | Duration | Service |
          | 2 days from today | 09:20 | 5        | FLU   |
        When I check daily availability for 'COVID' between 'Tomorrow' and '3 days from today'
        Then the following daily availability is returned
          | Date        | Am | Pm |
          | Tomorrow    | 12 | 0  |
          | 2 days from today | 12 | 0  |
          | 3 days from today | 12 | 0  |

    Scenario: Cancelled bookings of other services don't take up availability
      Given the following sessions
        | Date        | From  | Until | Services | Slot Length | Capacity |
        | Tomorrow    | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
        | 2 days from today | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
        | 3 days from today | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
      And the following cancelled bookings have been made
        | Date        | Time  | Duration | Service |
        | 2 days from today | 09:20 | 5        | FLU   |
      When I check daily availability for 'COVID' between 'Tomorrow' and '3 days from today'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 12 | 0  |
        | 2 days from today | 12 | 0  |
        | 3 days from today | 12 | 0  |
      
#    In a best fit scenario, all 4 bookings at 9:20 would have a slot, however, due to sub-optimal greedy allocation, 
#    the ABBA booking is orphaned, and the COVID/RSV 9:20 slot is still treated as available 
#    Ideally, in this test, all services would return 22 slots remaining
    Scenario: Greedy allocation alphabetical - suboptimal - availability is affected
      Given the following sessions
        | Date        | From  | Until | Services | Slot Length | Capacity |
        | Tomorrow    | 09:00 | 10:00 | COVID, RSV    | 5           | 1        |
        | Tomorrow    | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
        | Tomorrow    | 09:00 | 10:00 | ABBA, FLU    | 5           | 1        |
        | Tomorrow    | 09:00 | 10:00 | ABBA, RSV    | 5           | 1        |
      And the following bookings have been made
        | Date        | Time  | Duration | Service |
        | Tomorrow    | 09:20 | 5        | COVID   |
        | Tomorrow    | 09:20 | 5        | FLU   |
        | Tomorrow    | 09:20 | 5        | RSV   |
        | Tomorrow    | 09:20 | 5        | ABBA   |
      When I check daily availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 23 | 0  |
      When I check daily availability for 'FLU' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 22 | 0  |
      When I check daily availability for 'RSV' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 23 | 0  |
      When I check daily availability for 'ABBA' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 22 | 0  |
      
#   Due to optimal allocation, all the existing bookings are supported, so all services return 22 slots remaining
    Scenario: Greedy allocation alphabetical - optimal - availability is affected
      Given the following sessions
        | Date        | From  | Until | Services | Slot Length | Capacity |
        | Tomorrow    | 09:00 | 10:00 | COVID, RSV    | 5           | 1        |
        | Tomorrow    | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
        | Tomorrow    | 09:00 | 10:00 | ABBA, FLU    | 5           | 1        |
        | Tomorrow    | 09:00 | 10:00 | ABBA, RSV    | 5           | 1        |
      And the following bookings have been made
        | Date        | Time  | Duration | Service |
        | Tomorrow    | 09:20 | 5        | FLU   |
        | Tomorrow    | 09:20 | 5        | COVID   |
        | Tomorrow    | 09:20 | 5        | ABBA   |
        | Tomorrow    | 09:20 | 5        | RSV   |
      When I check daily availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 22 | 0  |
      When I check daily availability for 'FLU' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 22 | 0  |
      When I check daily availability for 'RSV' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 22 | 0  |
      When I check daily availability for 'ABBA' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 22 | 0  |

#    In a best fit scenario, all 4 bookings at 9:20 would have a slot, however, due to sub-optimal service length allocation, 
#    the FLU-B booking is orphaned, and the COVID/RSV/FLU/FLU-C/FLU-D/FLU-E 9:20 slot is still treated as available
    Scenario: Greedy allocation service lengths - suboptimal - availability is affected
      Given the following sessions
        | Date        | From  | Until | Services                                  | Slot Length | Capacity |
        | Tomorrow    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 5           | 1        |
        | Tomorrow    | 09:00 | 10:00 | COVID, FLU-B                              | 5           | 1        |
        | Tomorrow    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 5           | 1        |
        | Tomorrow    | 09:00 | 10:00 | FLU-B, RSV                                | 5           | 1        |
      And the following bookings have been made
        | Date        | Time  | Duration | Service |
        | Tomorrow    | 09:20 | 5        | COVID   |
        | Tomorrow    | 09:20 | 5        | FLU     |
        | Tomorrow    | 09:20 | 5        | RSV     |
        | Tomorrow    | 09:20 | 5        | FLU-B   |
      When I check daily availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 23 | 0  |
      When I check daily availability for 'FLU' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 23 | 0  |
      When I check daily availability for 'RSV' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 23 | 0  |
      When I check daily availability for 'FLU-B' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 33 | 0  |
      When I check daily availability for 'FLU-C' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 23 | 0  |
      When I check daily availability for 'FLU-D' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 12 | 0  |
      When I check daily availability for 'FLU-E' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 12 | 0  |
  
#   Due to optimal allocation, all the existing bookings are supported, so all services return minimal slots remaining
    Scenario: Greedy allocation service lengths - optimal - availability is affected
      Given the following sessions
        | Date        | From  | Until | Services                                  | Slot Length | Capacity |
        | Tomorrow    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 5           | 1        |
        | Tomorrow    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 5           | 1        |
        | Tomorrow    | 09:00 | 10:00 | FLU-B, RSV, FLU-C, FLU-D                  | 5           | 1        |
        | Tomorrow    | 09:00 | 10:00 | COVID, FLU-B                              | 5           | 1        |
      And the following bookings have been made
        | Date        | Time  | Duration | Service |
        | Tomorrow    | 09:20 | 5        | FLU-B   |
        | Tomorrow    | 09:20 | 5        | COVID   |
        | Tomorrow    | 09:20 | 5        | FLU     |
        | Tomorrow    | 09:20 | 5        | RSV     |
      When I check daily availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 22 | 0  |
      When I check daily availability for 'FLU' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 22 | 0  |
      When I check daily availability for 'RSV' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 22 | 0  |
      When I check daily availability for 'FLU-B' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 33 | 0  |
      When I check daily availability for 'FLU-C' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 33 | 0  |
      When I check daily availability for 'FLU-D' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 22 | 0  |
      When I check daily availability for 'FLU-E' between 'Tomorrow' and 'Tomorrow'
      Then the following daily availability is returned
        | Date        | Am | Pm |
        | Tomorrow    | 11 | 0  |
