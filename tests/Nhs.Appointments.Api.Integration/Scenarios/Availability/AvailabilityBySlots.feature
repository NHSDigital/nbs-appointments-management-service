Feature: Get available appointment slots

    Scenario: Slot availability is returned from session templates with 5 min appointments
        Given the following sessions exist for a created default site
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 09:30 | COVID    | 5           | 1        |
        When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow' at the default site
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 09:05 | 1     |
          | 09:05 | 09:10 | 1     |
          | 09:10 | 09:15 | 1     |
          | 09:15 | 09:20 | 1     |
          | 09:20 | 09:25 | 1     |
          | 09:25 | 09:30 | 1     |
          
    Scenario: Slot availability is returned from all day session
        Given the following sessions exist for a created default site
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 00:01 | 23:59 | COVID    | 30          | 1        |
        When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow' at the default site
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 00:01 | 00:31 | 1     |
          | 00:31 | 01:01 | 1     |
          | 01:01 | 01:31 | 1     |
          | 01:31 | 02:01 | 1     |
          | 02:01 | 02:31 | 1     |
          | 02:31 | 03:01 | 1     |
          | 03:01 | 03:31 | 1     |
          | 03:31 | 04:01 | 1     |
          | 04:01 | 04:31 | 1     |
          | 04:31 | 05:01 | 1     |
          | 05:01 | 05:31 | 1     |
          | 05:31 | 06:01 | 1     |
          | 06:01 | 06:31 | 1     |
          | 06:31 | 07:01 | 1     |
          | 07:01 | 07:31 | 1     |
          | 07:31 | 08:01 | 1     |
          | 08:01 | 08:31 | 1     |
          | 08:31 | 09:01 | 1     |
          | 09:01 | 09:31 | 1     |
          | 09:31 | 10:01 | 1     |
          | 10:01 | 10:31 | 1     |
          | 10:31 | 11:01 | 1     |
          | 11:01 | 11:31 | 1     |
          | 11:31 | 12:01 | 1     |
          | 12:01 | 12:31 | 1     |
          | 12:31 | 13:01 | 1     |
          | 13:01 | 13:31 | 1     |
          | 13:31 | 14:01 | 1     |
          | 14:01 | 14:31 | 1     |
          | 14:31 | 15:01 | 1     |
          | 15:01 | 15:31 | 1     |
          | 15:31 | 16:01 | 1     |
          | 16:01 | 16:31 | 1     |
          | 16:31 | 17:01 | 1     |
          | 17:01 | 17:31 | 1     |
          | 17:31 | 18:01 | 1     |
          | 18:01 | 18:31 | 1     |
          | 18:31 | 19:01 | 1     |
          | 19:01 | 19:31 | 1     |
          | 19:31 | 20:01 | 1     |
          | 20:01 | 20:31 | 1     |
          | 20:31 | 21:01 | 1     |
          | 21:01 | 21:31 | 1     |
          | 21:31 | 22:01 | 1     |
          | 22:01 | 22:31 | 1     |
          | 22:31 | 23:01 | 1     |
          | 23:01 | 23:31 | 1     |

    Scenario: Slot availability is returned from sessions with 5 min appointments and multiple capacity
        Given the following sessions exist for a created default site
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 09:30 | COVID    | 10          | 2        |
        When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow' at the default site
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 09:10 | 2     |
          | 09:10 | 09:20 | 2     |
          | 09:20 | 09:30 | 2     |

    Scenario: Slot availability is returned from overlapping sessions
        Given the following sessions exist for a created default site
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 09:30 | COVID    | 10          | 2        |
          | Tomorrow | 09:00 | 09:30 | COVID    | 15          | 3        |
        When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow' at the default site
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 09:10 | 2     |
          | 09:00 | 09:15 | 3     |
          | 09:10 | 09:20 | 2     |
          | 09:15 | 09:30 | 3     |
          | 09:20 | 09:30 | 2     |

    Scenario: Slot availability is returned for multiple days
        Given the following sessions exist for a created default site
          | Date       | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow   | 09:00 | 09:30 | COVID    | 10          | 1        |
          | 2 days from today | 10:30 | 11:00 | COVID    | 10          | 1        |
          | 3 days from today | 09:00 | 09:30 | COVID    | 15          | 1        |
        When I check slot availability for 'COVID' between 'Tomorrow' and '3 days from today' at the default site
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 09:10 | 1     |
          | 09:10 | 09:20 | 1     |
          | 09:20 | 09:30 | 1     |
        And the following availability is returned for '2 days from today'
          | From  | Until | Count |
          | 10:30 | 10:40 | 1     |
          | 10:40 | 10:50 | 1     |
          | 10:50 | 11:00 | 1     |
        And the following availability is returned for '3 days from today'
          | From  | Until | Count |
          | 09:00 | 09:15 | 1     |
          | 09:15 | 09:30 | 1     |

    Scenario: Booked appointments reduce capacity of the correct slot based on duration
        Given the following sessions exist for a created default site
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 09:30 | COVID    | 10          | 2        |
          | Tomorrow | 09:00 | 09:30 | COVID    | 15          | 3        |
        And the following bookings have been made at the default site
          | Date     | Time  | Duration | Service |
          | Tomorrow | 09:00 | 15       | COVID   |
        When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow' at the default site
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 09:10 | 2     |
          | 09:00 | 09:15 | 2     |
          | 09:10 | 09:20 | 2     |
          | 09:15 | 09:30 | 3     |
          | 09:20 | 09:30 | 2     |

    Scenario: Booked appointments reduce capacity correctly with similar sessions
        Given the following sessions exist for a created default site
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 09:30 | COVID    | 10          | 1        |
          | Tomorrow | 09:00 | 09:30 | COVID    | 10          | 1        |
        And the following bookings have been made at the default site
          | Date     | Time  | Duration | Service |
          | Tomorrow | 09:00 | 10       | COVID   |
        When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow' at the default site
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 09:10 | 1     |
          | 09:10 | 09:20 | 2     |
          | 09:20 | 09:30 | 2     |

    Scenario: Booked appointments of other service types reduce capcity of the slots
        Given the following sessions exist for a created default site
          | Date     | From  | Until | Services   | Slot Length | Capacity |
          | Tomorrow | 09:00 | 09:30 | COVID, FLU | 10          | 2        |
        And the following bookings have been made at the default site
          | Date     | Time  | Duration | Service |
          | Tomorrow | 09:00 | 10       | FLU     |
        When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow' at the default site
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 09:10 | 1     |
          | 09:10 | 09:20 | 2     |
          | 09:20 | 09:30 | 2     |

    Scenario: Booked appointments of other service types don't reduce capcity with multiple sessions
        Given the following sessions exist for a created default site
          | Date     | From  | Until | Services | Slot Length | Capacity |
          | Tomorrow | 09:00 | 09:30 | COVID    | 10          | 2        |
          | Tomorrow | 09:00 | 09:30 | FLU      | 10          | 2        |
        And the following bookings have been made at the default site
          | Date     | Time  | Duration | Service |
          | Tomorrow | 09:00 | 10       | FLU     |
        When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow' at the default site
        Then the following availability is returned for 'Tomorrow'
          | From  | Until | Count |
          | 09:00 | 09:10 | 2     |
          | 09:10 | 09:20 | 2     |
          | 09:20 | 09:30 | 2     |

    Scenario: Slot availability is only returned from sessions for that service
      Given the following sessions exist for a created default site
        | Date              | From  | Until | Services      | Slot Length | Capacity |
        | Tomorrow          | 09:00 | 10:00 | RSV           | 10           | 1        |
        | 2 days from today | 09:00 | 10:00 | RSV, COVID    | 10           | 1        |
        | 3 days from today | 09:00 | 10:00 | COVID         | 10           | 1        |
        | 4 days from today | 09:00 | 10:00 | COVID, FLU    | 10           | 1        |
      When I check slot availability for 'COVID' between 'Tomorrow' and '4 days from today' at the default site
      Then no availability is returned for 'Tomorrow'
      And the following availability is returned for '2 days from today'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
      And the following availability is returned for '3 days from today'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
      And the following availability is returned for '4 days from today'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
  
    Scenario: Slot availability is returned from overlapping sessions of different services
      Given the following sessions exist for a created default site
        | Date     | From  | Until | Services    | Slot Length  | Capacity |
        | Tomorrow | 09:00 | 10:00 | RSV, COVID  | 10            | 3        |
        | Tomorrow | 09:00 | 10:00 | COVID       | 10            | 2        |
        | Tomorrow | 09:00 | 10:00 | RSV         | 10            | 3        |
      When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 5     |
        | 09:10 | 09:20 | 5     |
        | 09:20 | 09:30 | 5     |
        | 09:30 | 09:40 | 5     |
        | 09:40 | 09:50 | 5     |
        | 09:50 | 10:00 | 5     |
  
    Scenario: Bookings of other services take up availability
      Given the following sessions exist for a created default site
        | Date              | From  | Until | Services      | Slot Length | Capacity |
        | Tomorrow          | 09:00 | 10:00 | COVID, FLU    | 10           | 1        |
        | 2 days from today | 09:00 | 10:00 | COVID, FLU    | 10           | 1        |
        | 3 days from today | 09:00 | 10:00 | COVID, FLU    | 10           | 1        |
      And the following bookings have been made at the default site
        | Date              | Time  | Duration | Service |
        | 2 days from today | 09:20 | 10        | FLU     |
      When I check slot availability for 'COVID' between 'Tomorrow' and '3 days from today' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
      And the following availability is returned for '2 days from today'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
      And the following availability is returned for '3 days from today'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
  
    Scenario: Provisional bookings of other services take up availability
      Given the following sessions exist for a created default site
        | Date              | From  | Until | Services      | Slot Length | Capacity |
        | Tomorrow          | 09:00 | 10:00 | COVID, FLU    | 10           | 1        |
        | 2 days from today | 09:00 | 10:00 | COVID, FLU    | 10           | 1        |
        | 3 days from today | 09:00 | 10:00 | COVID, FLU    | 10           | 1        |
      And the following provisional bookings have been made at the default site
        | Date              | Time  | Duration | Service |
        | 2 days from today | 09:20 | 10        | FLU     |
      When I check slot availability for 'COVID' between 'Tomorrow' and '3 days from today' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
      And the following availability is returned for '2 days from today'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
      And the following availability is returned for '3 days from today'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
  
    Scenario: Expired provisional bookings of other services don't take up availability
      Given the following sessions exist for a created default site
        | Date              | From  | Until | Services      | Slot Length | Capacity |
        | Tomorrow          | 09:00 | 10:00 | COVID, FLU    | 10           | 1        |
        | 2 days from today | 09:00 | 10:00 | COVID, FLU    | 10           | 1        |
        | 3 days from today | 09:00 | 10:00 | COVID, FLU    | 10           | 1        |
      And the following expired provisional bookings exist at the default site
        | Date              | Time  | Duration | Service |
        | 2 days from today | 09:20 | 10        | FLU     |
      When I check slot availability for 'COVID' between 'Tomorrow' and '3 days from today' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
      And the following availability is returned for '2 days from today'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
      And the following availability is returned for '3 days from today'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
  
    Scenario: Cancelled bookings of other services don't take up availability
      Given the following sessions exist for a created default site
        | Date              | From  | Until | Services      | Slot Length | Capacity |
        | Tomorrow          | 09:00 | 10:00 | COVID, FLU    | 10           | 1        |
        | 2 days from today | 09:00 | 10:00 | COVID, FLU    | 10           | 1        |
        | 3 days from today | 09:00 | 10:00 | COVID, FLU    | 10           | 1        |
      And the following cancelled bookings exist at the default site
        | Date              | Time  | Duration | Service |
        | 2 days from today | 09:20 | 10        | FLU     |
      When I check slot availability for 'COVID' between 'Tomorrow' and '3 days from today' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
      And the following availability is returned for '2 days from today'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
      And the following availability is returned for '3 days from today'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
      
  #    In a best fit scenario, all 4 bookings at 9:20 would have a slot, however, due to sub-optimal greedy allocation, 
  #    the ABBA booking is orphaned, and the COVID/RSV 9:20 slot is still treated as available
    Scenario: Greedy allocation alphabetical - suboptimal - availability is affected
      Given the following sessions exist for a created default site
        | Date        | From  | Until | Services | Slot Length | Capacity |
        | Tomorrow    | 09:00 | 10:00 | COVID, RSV    | 10           | 1        |
        | Tomorrow    | 09:00 | 10:00 | COVID, FLU    | 10           | 1        |
        | Tomorrow    | 09:00 | 10:00 | ABBA, FLU    | 10           | 1        |
        | Tomorrow    | 09:00 | 10:00 | ABBA, RSV    | 10           | 1        |
      And the following bookings have been made at the default site
        | Date        | Time  | Duration | Service |
        | Tomorrow    | 09:20 | 10        | COVID   |
        | Tomorrow    | 09:20 | 10        | FLU   |
        | Tomorrow    | 09:20 | 10        | RSV   |
        | Tomorrow    | 09:20 | 10        | ABBA   |
      When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 2     |
        | 09:10 | 09:20 | 2     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 2     |
        | 09:40 | 09:50 | 2     |
        | 09:50 | 10:00 | 2     |
      When I check slot availability for 'FLU' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 2     |
        | 09:10 | 09:20 | 2     |
        | 09:30 | 09:40 | 2     |
        | 09:40 | 09:50 | 2     |
        | 09:50 | 10:00 | 2     |
      When I check slot availability for 'RSV' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 2     |
        | 09:10 | 09:20 | 2     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 2     |
        | 09:40 | 09:50 | 2     |
        | 09:50 | 10:00 | 2     |
      When I check slot availability for 'ABBA' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 2     |
        | 09:10 | 09:20 | 2     |
        | 09:30 | 09:40 | 2     |
        | 09:40 | 09:50 | 2     |
        | 09:50 | 10:00 | 2     |
  
  #   Due to optimal allocation, all the existing bookings are supported
    Scenario: Greedy allocation alphabetical - optimal - availability is affected
      Given the following sessions exist for a created default site
        | Date        | From  | Until | Services | Slot Length | Capacity |
        | Tomorrow    | 09:00 | 10:00 | COVID, RSV    | 10           | 1        |
        | Tomorrow    | 09:00 | 10:00 | COVID, FLU    | 10           | 1        |
        | Tomorrow    | 09:00 | 10:00 | ABBA, FLU    | 10           | 1        |
        | Tomorrow    | 09:00 | 10:00 | ABBA, RSV    | 10           | 1        |
      And the following bookings have been made at the default site
        | Date        | Time  | Duration | Service |
        | Tomorrow    | 09:20 | 10        | FLU   |
        | Tomorrow    | 09:20 | 10        | COVID   |
        | Tomorrow    | 09:20 | 10        | ABBA   |
        | Tomorrow    | 09:20 | 10        | RSV   |
      When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 2     |
        | 09:10 | 09:20 | 2     |
        | 09:30 | 09:40 | 2     |
        | 09:40 | 09:50 | 2     |
        | 09:50 | 10:00 | 2     |
      When I check slot availability for 'FLU' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 2     |
        | 09:10 | 09:20 | 2     |
        | 09:30 | 09:40 | 2     |
        | 09:40 | 09:50 | 2     |
        | 09:50 | 10:00 | 2     |
      When I check slot availability for 'RSV' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 2     |
        | 09:10 | 09:20 | 2     |
        | 09:30 | 09:40 | 2     |
        | 09:40 | 09:50 | 2     |
        | 09:50 | 10:00 | 2     |
      When I check slot availability for 'ABBA' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 2     |
        | 09:10 | 09:20 | 2     |
        | 09:30 | 09:40 | 2     |
        | 09:40 | 09:50 | 2     |
        | 09:50 | 10:00 | 2     |
  
  #   In a best fit scenario, all 4 bookings at 9:20 would have a slot, however, due to sub-optimal service length allocation, 
  #   the FLU-B booking is orphaned, and the COVID/RSV/FLU/FLU-C/FLU-D/FLU-E 9:20 slot is still treated as available
    Scenario: Greedy allocation service lengths - suboptimal - availability is affected
      Given the following sessions exist for a created default site
        | Date        | From  | Until | Services                                  | Slot Length | Capacity |
        | Tomorrow    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
        | Tomorrow    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
        | Tomorrow    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
        | Tomorrow    | 09:00 | 10:00 | FLU-B, RSV                                | 10           | 1        |
      And the following bookings have been made at the default site
        | Date        | Time  | Duration | Service |
        | Tomorrow    | 09:20 | 10        | COVID   |
        | Tomorrow    | 09:20 | 10        | FLU     |
        | Tomorrow    | 09:20 | 10        | RSV     |
        | Tomorrow    | 09:20 | 10        | FLU-B   |
      When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 2     |
        | 09:10 | 09:20 | 2     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 2     |
        | 09:40 | 09:50 | 2     |
        | 09:50 | 10:00 | 2     |
      When I check slot availability for 'FLU' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 2     |
        | 09:10 | 09:20 | 2     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 2     |
        | 09:40 | 09:50 | 2     |
        | 09:50 | 10:00 | 2     |
      When I check slot availability for 'RSV' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 2     |
        | 09:10 | 09:20 | 2     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 2     |
        | 09:40 | 09:50 | 2     |
        | 09:50 | 10:00 | 2     |
      When I check slot availability for 'FLU-B' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 3     |
        | 09:10 | 09:20 | 3     |
        | 09:30 | 09:40 | 3     |
        | 09:40 | 09:50 | 3     |
        | 09:50 | 10:00 | 3     |
      When I check slot availability for 'FLU-C' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 2     |
        | 09:10 | 09:20 | 2     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 2     |
        | 09:40 | 09:50 | 2     |
        | 09:50 | 10:00 | 2     |
      When I check slot availability for 'FLU-D' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
      When I check slot availability for 'FLU-E' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:20 | 09:30 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
  
  #   Due to optimal allocation, all the existing bookings are supported, so all services return minimal slots remaining
    Scenario: Greedy allocation service lengths - optimal - availability is affected
      Given the following sessions exist for a created default site
        | Date        | From  | Until | Services                                  | Slot Length | Capacity |
        | Tomorrow    | 09:00 | 10:00 | COVID, RSV, FLU, FLU-C, FLU-D, FLU-E      | 10           | 1        |
        | Tomorrow    | 09:00 | 10:00 | FLU, FLU-B, FLU-C                         | 10           | 1        |
        | Tomorrow    | 09:00 | 10:00 | FLU-B, RSV, FLU-C, FLU-D                  | 10           | 1        |
        | Tomorrow    | 09:00 | 10:00 | COVID, FLU-B                              | 10           | 1        |
      And the following bookings have been made at the default site
        | Date        | Time  | Duration | Service |
        | Tomorrow    | 09:20 | 10        | FLU-B   |
        | Tomorrow    | 09:20 | 10        | COVID   |
        | Tomorrow    | 09:20 | 10        | FLU     |
        | Tomorrow    | 09:20 | 10        | RSV     |
      When I check slot availability for 'COVID' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 2     |
        | 09:10 | 09:20 | 2     |
        | 09:30 | 09:40 | 2     |
        | 09:40 | 09:50 | 2     |
        | 09:50 | 10:00 | 2     |
      When I check slot availability for 'FLU' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 2     |
        | 09:10 | 09:20 | 2     |
        | 09:30 | 09:40 | 2     |
        | 09:40 | 09:50 | 2     |
        | 09:50 | 10:00 | 2     |
      When I check slot availability for 'RSV' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 2     |
        | 09:10 | 09:20 | 2     |
        | 09:30 | 09:40 | 2     |
        | 09:40 | 09:50 | 2     |
        | 09:50 | 10:00 | 2     |
      When I check slot availability for 'FLU-B' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 3     |
        | 09:10 | 09:20 | 3     |
        | 09:30 | 09:40 | 3     |
        | 09:40 | 09:50 | 3     |
        | 09:50 | 10:00 | 3     |
      When I check slot availability for 'FLU-C' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 3     |
        | 09:10 | 09:20 | 3     |
        | 09:30 | 09:40 | 3     |
        | 09:40 | 09:50 | 3     |
        | 09:50 | 10:00 | 3     |
      When I check slot availability for 'FLU-D' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 2     |
        | 09:10 | 09:20 | 2     |
        | 09:30 | 09:40 | 2     |
        | 09:40 | 09:50 | 2     |
        | 09:50 | 10:00 | 2     |
      When I check slot availability for 'FLU-E' between 'Tomorrow' and 'Tomorrow' at the default site
      Then the following availability is returned for 'Tomorrow'
        | From  | Until | Count |
        | 09:00 | 09:10 | 1     |
        | 09:10 | 09:20 | 1     |
        | 09:30 | 09:40 | 1     |
        | 09:40 | 09:50 | 1     |
        | 09:50 | 10:00 | 1     |
        
