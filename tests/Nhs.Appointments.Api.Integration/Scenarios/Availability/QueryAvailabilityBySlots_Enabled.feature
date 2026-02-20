Feature: Query Availability By Slots

  Scenario: Returns 404 when site can't be found
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I query availability by slots at site '4353234234234'
      | Attendee Services   | Date     | From  | Until |
      | RSV:Adult           | Tomorrow | 09:00 | 17:00 |
    Then the call should fail with 404

  Scenario: Returns bad request when payload is invalid
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I pass an invalid payload
    Then the call should fail with 400

  Scenario: Returns bad request when too many attendees passed up
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I query availability by slots at the default site
      | Attendee Services                                           | Date     | From  | Until |
      | RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow | 09:00 | 17:00 |
    Then the call should fail with 400

  Scenario: Can return slot availabilty for a single attendee
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | RSV:Adult | 10          | 1        |
    When I query availability by slots at the default site
      | Attendee Services | Date     | From  | Until |
      | RSV:Adult         | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00' at the default site
      | From  | Until | Services  |
      | 09:00 | 09:10 | RSV:Adult |
      | 09:10 | 09:20 | RSV:Adult |
      | 09:20 | 09:30 | RSV:Adult |
      | 09:30 | 09:40 | RSV:Adult |
      | 09:40 | 09:50 | RSV:Adult |
      | 09:50 | 10:00 | RSV:Adult |

  Scenario: Can return slot availability for a single attendee over multiple sessions
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | RSV:Adult | 10          | 1        |
      | Tomorrow          | 12:00 | 13:00 | RSV:Adult | 10          | 1        |
    When I query availability by slots at the default site
      | Attendee Services | Date     | From  | Until |
      | RSV:Adult         | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00' at the default site
      | From  | Until | Services  |
      | 09:00 | 09:10 | RSV:Adult |
      | 09:10 | 09:20 | RSV:Adult |
      | 09:20 | 09:30 | RSV:Adult |
      | 09:30 | 09:40 | RSV:Adult |
      | 09:40 | 09:50 | RSV:Adult |
      | 09:50 | 10:00 | RSV:Adult |
      | 12:00 | 12:10 | RSV:Adult |
      | 12:10 | 12:20 | RSV:Adult |
      | 12:20 | 12:30 | RSV:Adult |
      | 12:30 | 12:40 | RSV:Adult |
      | 12:40 | 12:50 | RSV:Adult |
      | 12:50 | 13:00 | RSV:Adult |

  Scenario: Returns empty slot array when there is no availabilty for requested service
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | RSV:Adult | 10          | 1        |
    When I query availability by slots at the default site
      | Attendee Services | Date     | From  | Until |
      | COVID:5_11        | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00' at the default site
      | From  | Until | Services |

  Scenario: Returns correct slot availability for slots that spillover an hour
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:10 | 10:10 | RSV:Adult | 15          | 1        |
    When I query availability by slots at the default site
      | Attendee Services | Date     | From  | Until |
      | RSV:Adult         | Tomorrow | 09:00 | 10:10 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:10' at the default site
      | From  | Until | Services  |
      | 09:10 | 09:25 | RSV:Adult |
      | 09:25 | 09:40 | RSV:Adult |
      | 09:40 | 09:55 | RSV:Adult |
      | 09:55 | 10:10 | RSV:Adult |

  Scenario: Returns empty slots array when that aren't enough slots for multiple attendees
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | RSV:Adult | 15          | 1        |
    When I query availability by slots at the default site
      | Attendee Services                                 | Date     | From  | Until |
      | RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00' at the default site
      | From  | Until | Services |

  Scenario: Returns correct slot availability for multiple attendees requesting a single service
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | RSV:Adult | 10          | 1        |
    When I query availability by slots at the default site
      | Attendee Services   | Date     | From  | Until |
      | RSV:Adult,RSV:Adult | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00' at the default site
      | From  | Until | Services  |
      | 09:00 | 09:10 | RSV:Adult |
      | 09:10 | 09:20 | RSV:Adult |
      | 09:20 | 09:30 | RSV:Adult |
      | 09:30 | 09:40 | RSV:Adult |
      | 09:40 | 09:50 | RSV:Adult |
      | 09:50 | 10:00 | RSV:Adult |

  Scenario: Returns correct slot availabiltiy for multiple attendees requesting two services
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 09:30 | 10:30 | COVID:5_11 | 10          | 1        |
    When I query availability by slots at the default site
      | Attendee Services    | Date     | From  | Until |
      | RSV:Adult,COVID:5_11 | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00' at the default site
      | From  | Until | Services   |
      | 09:20 | 09:30 | RSV:Adult  |
      | 09:30 | 09:40 | RSV:Adult  |
      | 09:40 | 09:50 | RSV:Adult  |
      | 09:50 | 10:00 | RSV:Adult  |
      | 09:30 | 09:40 | COVID:5_11 |
      | 09:40 | 09:50 | COVID:5_11 |
      | 09:50 | 10:00 | COVID:5_11 |
      | 10:00 | 10:10 | COVID:5_11 |

  Scenario: Returns empty slot array when sessions don't overlap for multiple attendees requesting multiple services
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:30 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 09:31 | 10:30 | COVID:5_11 | 10          | 1        |
    When I query availability by slots at the default site
      | Attendee Services    | Date     | From  | Until |
      | RSV:Adult,COVID:5_11 | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00' at the default site
      | From  | Until | Services   |

  Scenario: Returns correct slots for overlapping sessions
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:28 | RSV:Adult  | 7           | 1        |
      | Tomorrow          | 09:00 | 09:30 | COVID:5_11 | 3           | 1        |
    When I query availability by slots at the default site
      | Attendee Services    | Date     | From  | Until |
      | RSV:Adult,COVID:5_11 | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00' at the default site
      | From  | Until | Services   |
      | 09:18 | 09:21 | COVID:5_11 |
      | 09:21 | 09:28 | RSV:Adult  |
      | 09:14 | 09:21 | RSV:Adult  |
      | 09:21 | 09:24 | COVID:5_11 |

  Scenario: Returns slots that are just outside the hour range if they are suitable for a joint booking
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 08:50 | 10:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 09:00 | 10:10 | COVID:5_11 | 10          | 1        |
    When I query availability by slots at the default site
      | Attendee Services    | Date     | From  | Until |
      | RSV:Adult,COVID:5_11 | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00' at the default site
      | From  | Until | Services   |
      | 08:50 | 09:00 | RSV:Adult  |
      | 09:00 | 09:10 | RSV:Adult  |
      | 09:10 | 09:20 | RSV:Adult  |
      | 09:20 | 09:30 | RSV:Adult  |
      | 09:30 | 09:40 | RSV:Adult  |
      | 09:40 | 09:50 | RSV:Adult  |
      | 09:50 | 10:00 | RSV:Adult  |
      | 09:00 | 09:10 | COVID:5_11 |
      | 09:10 | 09:20 | COVID:5_11 |
      | 09:20 | 09:30 | COVID:5_11 |
      | 09:30 | 09:40 | COVID:5_11 |
      | 09:40 | 09:50 | COVID:5_11 |
      | 09:50 | 10:00 | COVID:5_11 |
      | 10:00 | 10:10 | COVID:5_11 |

  Scenario: Returns correct slots when there is just enough availability for four attendees
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | RSV:Adult | 15          | 1        |
    When I query availability by slots at the default site
      | Attendee Services                       | Date     | From  | Until |
      | RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00' at the default site
      | From  | Until | Services   |
      | 09:00 | 09:15 | RSV:Adult  |
      | 09:15 | 09:30 | RSV:Adult  |
      | 09:30 | 09:45 | RSV:Adult  |
      | 09:45 | 10:00 | RSV:Adult  |

  Scenario: Only return slots that can accommodate three attendees requesting the same service
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow | 09:00 | 09:30 | RSV:Adult | 15          | 1        |
      | Tomorrow | 12:00 | 13:00 | RSV:Adult | 15          | 1        |
    When I query availability by slots at the default site
      | Attendee Services             | Date     | From  | Until |
      | RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow | 09:00 | 17:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '17:00' at the default site
      | From  | Until | Services   |
      | 12:00 | 12:15 | RSV:Adult  |
      | 12:15 | 12:30 | RSV:Adult  |
      | 12:30 | 12:45 | RSV:Adult  |
      | 12:45 | 13:00 | RSV:Adult  |

  Scenario: Only return slots that can accommodate three attendees requesting two services
    Given the following sessions exist for a created default site
      | Date     | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow | 09:00 | 09:45 | RSV:Adult  | 15          | 1        |
      | Tomorrow | 12:00 | 13:00 | RSV:Adult  | 15          | 1        |
      | Tomorrow | 12:00 | 13:00 | COVID:5_11 | 15          | 1        |
    When I query availability by slots at the default site
      | Attendee Services              | Date     | From  | Until |
      | RSV:Adult,RSV:Adult,COVID:5_11 | Tomorrow | 09:00 | 17:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '17:00' at the default site
      | From  | Until | Services   |
      | 12:00 | 12:15 | RSV:Adult  |
      | 12:15 | 12:30 | RSV:Adult  |
      | 12:30 | 12:45 | RSV:Adult  |
      | 12:45 | 13:00 | RSV:Adult  |
      | 12:00 | 12:15 | COVID:5_11 |
      | 12:15 | 12:30 | COVID:5_11 |
      | 12:30 | 12:45 | COVID:5_11 |
      | 12:45 | 13:00 | COVID:5_11 |
      
  Scenario: One booked slot breaks a consecutive pair
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:30 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made at the default site
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 09:10 | 10       | RSV:Adult | 56345-11111 |
    When I query availability by slots at the default site
      | Attendee Services   | Date     | From  | Until |
      | RSV:Adult,RSV:Adult | Tomorrow | 09:00 | 09:30 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '09:30' at the default site
      | From  | Until | Services   |

  Scenario: Booking at the start still allows later consecutive slots
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:30 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made at the default site
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 09:00 | 10       | RSV:Adult | 56345-11111 |
    When I query availability by slots at the default site
      | Attendee Services   | Date     | From  | Until |
      | RSV:Adult,RSV:Adult | Tomorrow | 09:00 | 09:30 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '09:30' at the default site
      | From  | Until | Services   |
      | 09:10 | 09:20 | RSV:Adult  |
      | 09:20 | 09:30 | RSV:Adult  |

  Scenario: Booking at the end still allows earlier consecutive slots
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:30 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made at the default site
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 09:20 | 10       | RSV:Adult | 56345-11111 |
    When I query availability by slots at the default site
      | Attendee Services   | Date     | From  | Until |
      | RSV:Adult,RSV:Adult | Tomorrow | 09:00 | 09:30 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '09:30' at the default site
      | From  | Until | Services   |
      | 09:00 | 09:10 | RSV:Adult  |
      | 09:10 | 09:20 | RSV:Adult  |

  Scenario: Two non-adjacent bookings split availability into two valid windows
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:50 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made at the default site
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 09:10 | 10       | RSV:Adult | 56345-11111 |
      | Tomorrow    | 09:30 | 10       | RSV:Adult | 12345-11111 |
    When I query availability by slots at the default site
      | Attendee Services   | Date     | From  | Until |
      | RSV:Adult,RSV:Adult | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00' at the default site
      | From  | Until | Services   |

  Scenario: One booking leaves valid consecutive pairs
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 11:40 | 12:30 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made at the default site
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 12:00 | 10       | RSV:Adult | 56345-11111 |
    When I query availability by slots at the default site
      | Attendee Services   | Date     | From  | Until |
      | RSV:Adult,RSV:Adult | Tomorrow | 11:00 | 13:00 |
    Then the following availability is returned for 'Tomorrow' between '11:00' and '13:00' at the default site
      | From  | Until | Services   |
      | 11:40 | 11:50 | RSV:Adult  |
      | 11:50 | 12:00 | RSV:Adult  |
      | 12:10 | 12:20 | RSV:Adult  |
      | 12:20 | 12:30 | RSV:Adult  |

  Scenario: Only two slots exist and one is booked
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:20 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made at the default site
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 09:00 | 10       | RSV:Adult | 56345-11111 |
    When I query availability by slots at the default site
      | Attendee Services   | Date     | From  | Until |
      | RSV:Adult,RSV:Adult | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00' at the default site
      | From  | Until | Services   |

  Scenario: One session too short, another session valid
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:10 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 13:00 | 13:20 | RSV:Adult  | 10          | 1        |
    When I query availability by slots at the default site
      | Attendee Services   | Date     | From  | Until |
      | RSV:Adult,RSV:Adult | Tomorrow | 09:00 | 14:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '14:00' at the default site
      | From  | Until | Services   |
      | 13:00 | 13:10 | RSV:Adult  |
      | 13:10 | 13:20 | RSV:Adult  |
