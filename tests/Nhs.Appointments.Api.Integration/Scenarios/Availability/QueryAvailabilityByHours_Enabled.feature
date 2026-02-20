Feature: Query Availability By Hours

  Scenario: Returns 404 when site can't be found
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I query availability by hours for site 'e8f0c55a-571b-4538-a073-a626cca53547'
      | Attendee Services   | Date     |
      | RSV:Adult           | Tomorrow |
    Then the call should fail with 404

  Scenario: Returns bad request when payload is invalid
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I pass an invalid payload
    Then the call should fail with 400

  Scenario: Returns bad request when too many attendees passed up
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I query availability by hours at the default site
      | Attendee Services                                           | Date     |
      | RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow |
    Then the call should fail with 400

  Scenario: Can return availability for single attendee
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    When I query availability by hours at the default site
      | Attendee Services | Date     |
      | RSV:Adult         | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |
      | 09:00 | 10:00 |
      | 10:00 | 11:00 |
      | 11:00 | 12:00 |

  Scenario: Can return availability for single attendee over multiple sessions
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
      | Tomorrow          | 14:00 | 16:00 | RSV:Adult | 10          | 1        |
    When I query availability by hours at the default site
      | Attendee Services | Date     |
      | RSV:Adult         | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |
      | 09:00 | 10:00 |
      | 10:00 | 11:00 |
      | 11:00 | 12:00 |
      | 14:00 | 15:00 |
      | 15:00 | 16:00 |

  Scenario: Returns empty hours array when there is no availability for requested service
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    When I query availability by hours at the default site
      | Attendee Services | Date     |
      | COVID:5_11        | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |

  Scenario: Correctly displays midnight time
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 21:00 | 23:30 | RSV:Adult | 10          | 1        |
    When I query availability by hours at the default site
      | Attendee Services | Date     |
      | RSV:Adult         | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |
      | 21:00 | 22:00 |
      | 22:00 | 23:00 |
      | 23:00 | 00:00 |

  Scenario: Only returns hour blocks from start time of slot and doesn't include spillover hour
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 10:55 | 11:05 | RSV:Adult | 10          | 1        |
    When I query availability by hours at the default site
      | Attendee Services | Date     |
      | RSV:Adult         | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |
      | 10:00 | 11:00 |

  Scenario: Returns correct hour blocks for two attendees requesting a single service
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 13:00 | RSV:Adult | 10          | 1        |
    When I query availability by hours at the default site
      | Attendee Services   | Date     |
      | RSV:Adult,RSV:Adult | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |
      | 10:00 | 11:00 |
      | 11:00 | 12:00 |
      | 12:00 | 13:00 |

  Scenario: Returns empty hours array when there aren't enough slots for multiple attendees
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 11:00 | RSV:Adult | 15          | 1        |
    When I query availability by hours at the default site
      | Attendee Services                                 | Date     |
      | RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |

  Scenario: Correctly returns hour blocks for two attendees requesting two services
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 13:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 11:00 | 13:00 | COVID:5_11 | 10          | 1        |
    When I query availability by hours at the default site
      | Attendee Services    | Date     |
      | RSV:Adult,COVID:5_11 | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |
      | 10:00 | 11:00 |
      | 11:00 | 12:00 |
      | 12:00 | 13:00 |

  Scenario: Returns matching slots of different lengths for two attendees request two services
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 13:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 12:00 | 13:00 | COVID:5_11 | 5           | 1        |
    When I query availability by hours at the default site
      | Attendee Services    | Date     |
      | RSV:Adult,COVID:5_11 | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |
      | 11:00 | 12:00 |
      | 12:00 | 13:00 |

  Scenario: Just enough availability for the amount of attendees requested
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 11:00 | RSV:Adult  | 15          | 1        |
    When I query availability by hours at the default site
      | Attendee Services                       | Date     |
      | RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |
      | 10:00 | 11:00 |

  Scenario: Returns empty hours array when neighbouring slots don't exactly match up
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 13:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 13:01 | 14:00 | COVID:5_11 | 5           | 1        |
    When I query availability by hours at the default site
      | Attendee Services    | Date     |
      | RSV:Adult,COVID:5_11 | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |

  Scenario: Returns availability for 3 attendees requesting a single service
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 13:00 | RSV:Adult  | 10          | 1        |
    When I query availability by hours at the default site
      | Attendee Services             | Date     |
      | RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |
      | 10:00 | 11:00 |
      | 11:00 | 12:00 |
      | 12:00 | 13:00 |

  Scenario: Returns availability for 3 attendees requesting two services
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 13:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 12:00 | 14:00 | COVID:5_11 | 5           | 1        |
    When I query availability by hours at the default site
      | Attendee Services              | Date     |
      | RSV:Adult,COVID:5_11,RSV:Adult | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |
      | 11:00 | 12:00 |
      | 12:00 | 13:00 |
      | 13:00 | 14:00 |

  Scenario: Returns availability for 3 attendees requesting three services
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 13:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 12:00 | 14:00 | COVID:5_11 | 5           | 1        |
      | Tomorrow          | 11:00 | 14:00 | FLU:2_3    | 5           | 1        |
    When I query availability by hours at the default site
      | Attendee Services            | Date     |
      | RSV:Adult,COVID:5_11,FLU:2_3 | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |
      | 11:00 | 12:00 |
      | 12:00 | 13:00 |
      | 13:00 | 14:00 |

  Scenario: Returns empty hours array when only one requested service matches
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 13:00 | RSV:Adult  | 10          | 1        |
    When I query availability by hours at the default site
      | Attendee Services            | Date     |
      | RSV:Adult,COVID:5_11,FLU:2_3 | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |

  Scenario: Returns empty hours array when only 2 of 3 requested services match
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 12:00 | 14:00 | COVID:5_11 | 5           | 1        |
      | Tomorrow          | 10:00 | 13:00 | RSV:Adult  | 10          | 1        |
    When I query availability by hours at the default site
      | Attendee Services            | Date     |
      | RSV:Adult,COVID:5_11,FLU:2_3 | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |

  Scenario: One booked slot breaks a consecutive pair
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:30 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made at the default site
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 09:10 | 10       | RSV:Adult | 56345-11111 |
    When I query availability by hours at the default site
      | Attendee Services   | Date     |
      | RSV:Adult,RSV:Adult | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |

  Scenario: Booking at the start still allows later consecutive slots
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:30 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made at the default site
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 09:00 | 10       | RSV:Adult | 56345-11111 |
    When I query availability by hours at the default site
      | Attendee Services   | Date     |
      | RSV:Adult,RSV:Adult | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |
      | 09:00 | 10:00 |

  Scenario: Booking at the end still allows earlier consecutive slots
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:30 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made at the default site
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 09:20 | 10       | RSV:Adult | 56345-11111 |
    When I query availability by hours at the default site
      | Attendee Services   | Date     |
      | RSV:Adult,RSV:Adult | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |
      | 09:00 | 10:00 |

  Scenario: Two non-adjacent bookings split availability into two valid windows
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:50 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made at the default site
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 09:10 | 10       | RSV:Adult | 56345-11111 |
      | Tomorrow    | 09:30 | 10       | RSV:Adult | 12345-11111 |
    When I query availability by hours at the default site
      | Attendee Services   | Date     |
      | RSV:Adult,RSV:Adult | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |

  Scenario: One booking leaves valid consecutive pairs
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 11:40 | 12:30 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made at the default site
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 12:00 | 10       | RSV:Adult | 56345-11111 |
    When I query availability by hours at the default site
      | Attendee Services   | Date     |
      | RSV:Adult,RSV:Adult | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |
      | 11:00 | 12:00 |
      | 12:00 | 13:00 |

  Scenario: Only two slots exist and one is booked
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:20 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made at the default site
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 09:00 | 10       | RSV:Adult | 56345-11111 |
    When I query availability by hours at the default site
      | Attendee Services   | Date     |
      | RSV:Adult,RSV:Adult | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |

  Scenario: One session too short, another session valid
    Given the following sessions exist for a created default site
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:10 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 13:00 | 13:20 | RSV:Adult  | 10          | 1        |
    When I query availability by hours at the default site
      | Attendee Services   | Date     |
      | RSV:Adult,RSV:Adult | Tomorrow |
    Then the following 'RSV:Adult' availability is returned for 'Tomorrow' at the default site
      | From  | Until |
      | 13:00 | 14:00 |
