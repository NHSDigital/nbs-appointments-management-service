﻿Feature: Get hourly availability

    Scenario: Hourly availability is returned from session templates with 5 min appointments
        Given the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 11:00 | COVID    | 5           | 1        |
        When I check hourly availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 10:00 | 12    |
          | 10:00 | 11:00 | 12    |

    Scenario: Hourly availability is returned from sessions with 5 min appointments and multiple capacity
        Given the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 11:00 | COVID    | 10          | 2        |
        When I check hourly availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 10:00 | 12    |
          | 10:00 | 11:00 | 12    |

    Scenario: Hourly availability is returned when sessions cover every hour
        Given the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 00:01 | 23:59 | COVID    | 10          | 2        |
        When I check hourly availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 00:00 | 01:00 | 30    |
          | 01:00 | 02:00 | 30    |
          | 02:00 | 03:00 | 30    |
          | 03:00 | 04:00 | 30    |
          | 05:00 | 06:00 | 30    |
          | 06:00 | 07:00 | 30    |
          | 07:00 | 08:00 | 30    |
          | 08:00 | 09:00 | 30    |
          | 09:00 | 10:00 | 30    |
          | 10:00 | 11:00 | 30    |
          | 11:00 | 12:00 | 30    |
          | 12:00 | 13:00 | 30    |
          | 13:00 | 14:00 | 30    |
          | 14:00 | 15:00 | 30    |
          | 16:00 | 17:00 | 30    |
          | 17:00 | 18:00 | 30    |
          | 18:00 | 19:00 | 30    |
          | 19:00 | 20:00 | 30    |
          | 20:00 | 21:00 | 30    |
          | 21:00 | 22:00 | 30    |
          | 22:00 | 23:00 | 30    |
          | 23:00 | 00:00 | 30    |

    Scenario: Hourly availability is returned from overlapping sessions
        Given the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 11:00 | COVID    | 10          | 2        |
          | Tomorrow | 09:00 | 11:00 | COVID    | 10          | 3        |
        When I check hourly availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 10:00 | 30    |
          | 10:00 | 11:00 | 30    |

    Scenario: Hourly availability is returned for multiple days
        Given the following sessions
          | Date       | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow   | 09:00 | 13:00 | COVID    | 5           | 1        |
          | 2 days from today | 09:00 | 13:00 | COVID    | 5           | 1        |
          | 3 days from today | 09:00 | 13:00 | COVID    | 5           | 1        |
        When I check hourly availability for 'COVID' between 'Tomorrow' and '3 days from today'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 10:00 | 12    |
          | 10:00 | 11:00 | 12    |
          | 11:00 | 12:00 | 12    |
          | 12:00 | 13:00 | 12    |
        And the following availability is returned for '2 days from today'
          | From  | Until | Count |
          | 09:00 | 10:00 | 12    |
          | 10:00 | 11:00 | 12    |
          | 11:00 | 12:00 | 12    |
          | 12:00 | 13:00 | 12    |
        And the following availability is returned for '3 days from today'
          | From  | Until | Count |
          | 09:00 | 10:00 | 12    |
          | 10:00 | 11:00 | 12    |
          | 11:00 | 12:00 | 12    |
          | 12:00 | 13:00 | 12    |

    Scenario: Hourly availability is returned in ascending order
        Given the following sessions
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 10:00 | COVID    | 5           | 1        |
          | Tomorrow | 07:00 | 08:00 | COVID    | 5           | 1        |
        When I check hourly availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 07:00 | 08:00 | 12    |
          | 09:00 | 10:00 | 12    |
  
    Scenario: Hourly availability is only returned from sessions for that service
      Given the following sessions
        | Date              | From  | Until | Services      | Slot Length | Capacity |
        | Tomorrow          | 09:00 | 11:00 | RSV           | 5           | 1        |
        | 2 days from today | 09:00 | 11:00 | RSV, COVID    | 5           | 1        |
        | 3 days from today | 09:00 | 11:00 | COVID         | 5           | 1        |
        | 4 days from today | 09:00 | 11:00 | COVID, FLU    | 5           | 1        |
      When I check hourly availability for 'COVID' between 'Tomorrow' and '4 days from today'
      Then no availability is returned for 'Tomorrow'
      And the following availability is returned for '2 days from today'
        | From  | Until | Count |
        | 09:00 | 10:00 | 12    |
        | 10:00 | 11:00 | 12    |
      And the following availability is returned for '3 days from today'
        | From  | Until | Count |
        | 09:00 | 10:00 | 12    |
        | 10:00 | 11:00 | 12    |
      And the following availability is returned for '4 days from today'
        | From  | Until | Count |
        | 09:00 | 10:00 | 12    |
        | 10:00 | 11:00 | 12    |
  
    Scenario: Hourly availability is returned from overlapping sessions of different services
      Given the following sessions
        | Date     | From  | Until | Services    | Slot Length  | Capacity |
        | Tomorrow | 09:00 | 10:00 | RSV, COVID  | 5            | 3        |
        | Tomorrow | 09:00 | 10:00 | COVID       | 5            | 2        |
        | Tomorrow | 09:00 | 10:00 | RSV         | 5            | 3        |
      When I check hourly availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 60    |
  
    Scenario: Bookings of other services take up availability
      Given the following sessions
        | Date              | From  | Until | Services      | Slot Length | Capacity |
        | Tomorrow          | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
        | 2 days from today | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
        | 3 days from today | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
      And the following bookings have been made
        | Date              | Time  | Duration | Service |
        | 2 days from today | 09:20 | 5        | FLU     |
      When I check hourly availability for 'COVID' between 'Tomorrow' and '3 days from today'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 12    |
      And the following availability is returned for '2 days from today'
        | From  | Until | Count |
        | 09:00 | 10:00 | 11    |
      And the following availability is returned for '3 days from today'
        | From  | Until | Count |
        | 09:00 | 10:00 | 12    |
  
    Scenario: Provisional bookings of other services take up availability
      Given the following sessions
        | Date              | From  | Until | Services      | Slot Length | Capacity |
        | Tomorrow          | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
        | 2 days from today | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
        | 3 days from today | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
      And the following provisional bookings have been made
        | Date              | Time  | Duration | Service |
        | 2 days from today | 09:20 | 5        | FLU     |
      When I check hourly availability for 'COVID' between 'Tomorrow' and '3 days from today'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 12    |
      And the following availability is returned for '2 days from today'
        | From  | Until | Count |
        | 09:00 | 10:00 | 11    |
      And the following availability is returned for '3 days from today'
        | From  | Until | Count |
        | 09:00 | 10:00 | 12    |
  
    Scenario: Expired provisional bookings of other services don't take up availability
      Given the following sessions
        | Date              | From  | Until | Services      | Slot Length | Capacity |
        | Tomorrow          | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
        | 2 days from today | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
        | 3 days from today | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
      And the following expired provisional bookings have been made
        | Date              | Time  | Duration | Service |
        | 2 days from today | 09:20 | 5        | FLU     |
      When I check hourly availability for 'COVID' between 'Tomorrow' and '3 days from today'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 12    |
      And the following availability is returned for '2 days from today'
        | From  | Until | Count |
        | 09:00 | 10:00 | 12    |
      And the following availability is returned for '3 days from today'
        | From  | Until | Count |
        | 09:00 | 10:00 | 12    |
  
    Scenario: Cancelled bookings of other services don't take up availability
      Given the following sessions
        | Date              | From  | Until | Services      | Slot Length | Capacity |
        | Tomorrow          | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
        | 2 days from today | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
        | 3 days from today | 09:00 | 10:00 | COVID, FLU    | 5           | 1        |
      And the following cancelled bookings have been made
        | Date              | Time  | Duration | Service |
        | 2 days from today | 09:20 | 5        | FLU     |
      When I check hourly availability for 'COVID' between 'Tomorrow' and '3 days from today'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 12    |
      And the following availability is returned for '2 days from today'
        | From  | Until | Count |
        | 09:00 | 10:00 | 12    |
      And the following availability is returned for '3 days from today'
        | From  | Until | Count |
        | 09:00 | 10:00 | 12    |
        
        
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
      When I check hourly availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 23    |
      When I check hourly availability for 'FLU' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 22    |
      When I check hourly availability for 'RSV' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 23    |
      When I check hourly availability for 'ABBA' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 22    |
        
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
      When I check hourly availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 22    |
      When I check hourly availability for 'FLU' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 22    |
      When I check hourly availability for 'RSV' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 22    |
      When I check hourly availability for 'ABBA' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 22    |
        
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
      When I check hourly availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 23    |
      When I check hourly availability for 'FLU' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 23    |
      When I check hourly availability for 'RSV' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 23    |
      When I check hourly availability for 'FLU-B' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 33    |
      When I check hourly availability for 'FLU-C' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 23    |
      When I check hourly availability for 'FLU-D' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 12    |
      When I check hourly availability for 'FLU-E' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 12    |
        
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
      When I check hourly availability for 'COVID' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 22    |
      When I check hourly availability for 'FLU' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 22    |
      When I check hourly availability for 'RSV' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 22    |
      When I check hourly availability for 'FLU-B' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 33    |
      When I check hourly availability for 'FLU-C' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 33    |
      When I check hourly availability for 'FLU-D' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 22    |
      When I check hourly availability for 'FLU-E' between 'Tomorrow' and 'Tomorrow'
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 10:00 | 11    |
        
