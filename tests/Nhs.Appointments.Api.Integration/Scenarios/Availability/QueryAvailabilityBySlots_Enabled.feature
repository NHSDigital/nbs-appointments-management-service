Feature: Query Availability By Slots

  Scenario: Returns 404 when site can't be found
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 41b5fc18-0115-4f84-a780-af5a3025c6fe | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '41b5fc18-0115-4f84-a780-af5a3025c6fe'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I query availability by slots
      | Site                                 | Attendee Services   | Date     | From  | Until |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | RSV:Adult           | Tomorrow | 09:00 | 17:00 |
    Then the call should fail with 404

  Scenario: Returns bad request when payload is invalid
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 41b5fc18-0115-4f84-a780-af5a3025c6fe | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '41b5fc18-0115-4f84-a780-af5a3025c6fe'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I pass an invalid payload
    Then the call should fail with 400

  Scenario: Returns bad request when too many attendees passed up
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 41b5fc18-0115-4f84-a780-af5a3025c6fe | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '41b5fc18-0115-4f84-a780-af5a3025c6fe'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I query availability by slots
      | Site                                 | Attendee Services                                           | Date     | From  | Until |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow | 09:00 | 17:00 |
    Then the call should fail with 400

  Scenario: Can return slot availabilty for a single attendee
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '8538fc5b-e6fa-4924-a0d4-dcc47ebaa421'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | RSV:Adult | 10          | 1        |
    When I query availability by slots
      | Site                                 | Attendee Services | Date     | From  | Until |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | RSV:Adult         | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00'
      | From  | Until | Services  |
      | 09:00 | 09:10 | RSV:Adult |
      | 09:10 | 09:20 | RSV:Adult |
      | 09:20 | 09:30 | RSV:Adult |
      | 09:30 | 09:40 | RSV:Adult |
      | 09:40 | 09:50 | RSV:Adult |
      | 09:50 | 10:00 | RSV:Adult |

  Scenario: Can return slot availability for a single attendee over multiple sessions
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '8538fc5b-e6fa-4924-a0d4-dcc47ebaa421'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | RSV:Adult | 10          | 1        |
      | Tomorrow          | 12:00 | 13:00 | RSV:Adult | 10          | 1        |
    When I query availability by slots
      | Site                                 | Attendee Services | Date     | From  | Until |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | RSV:Adult         | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00'
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
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '8538fc5b-e6fa-4924-a0d4-dcc47ebaa421'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | RSV:Adult | 10          | 1        |
    When I query availability by slots
      | Site                                 | Attendee Services | Date     | From  | Until |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | COVID:5_11        | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00'
      | From  | Until | Services |

  Scenario: Returns correct slot availability for slots that spillover an hour
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '8538fc5b-e6fa-4924-a0d4-dcc47ebaa421'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:10 | 10:10 | RSV:Adult | 15          | 1        |
    When I query availability by slots
      | Site                                 | Attendee Services | Date     | From  | Until |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | RSV:Adult         | Tomorrow | 09:00 | 10:10 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:10'
      | From  | Until | Services  |
      | 09:10 | 09:25 | RSV:Adult |
      | 09:25 | 09:40 | RSV:Adult |
      | 09:40 | 09:55 | RSV:Adult |
      | 09:55 | 10:10 | RSV:Adult |

  Scenario: Returns empty slots array when that aren't enough slots for multiple attendees
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '8538fc5b-e6fa-4924-a0d4-dcc47ebaa421'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | RSV:Adult | 15          | 1        |
    When I query availability by slots
      | Site                                 | Attendee Services                                 | Date     | From  | Until |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00'
      | From  | Until | Services |

  Scenario: Returns correct slot availability for multiple attendees requesting a single service
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '8538fc5b-e6fa-4924-a0d4-dcc47ebaa421'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | RSV:Adult | 10          | 1        |
    When I query availability by slots
      | Site                                 | Attendee Services   | Date     | From  | Until |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | RSV:Adult,RSV:Adult | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00'
      | From  | Until | Services  |
      | 09:00 | 09:10 | RSV:Adult |
      | 09:10 | 09:20 | RSV:Adult |
      | 09:20 | 09:30 | RSV:Adult |
      | 09:30 | 09:40 | RSV:Adult |
      | 09:40 | 09:50 | RSV:Adult |
      | 09:50 | 10:00 | RSV:Adult |

  Scenario: Returns correct slot availabiltiy for multiple attendees requesting two services
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '8538fc5b-e6fa-4924-a0d4-dcc47ebaa421'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 10:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 09:30 | 10:30 | COVID:5_11 | 10          | 1        |
    When I query availability by slots
      | Site                                 | Attendee Services    | Date     | From  | Until |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | RSV:Adult,COVID:5_11 | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00'
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
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '8538fc5b-e6fa-4924-a0d4-dcc47ebaa421'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:30 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 09:31 | 10:30 | COVID:5_11 | 10          | 1        |
    When I query availability by slots
      | Site                                 | Attendee Services    | Date     | From  | Until |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | RSV:Adult,COVID:5_11 | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00'
      | From  | Until | Services   |

  Scenario: Returns correct slots for overlapping sessions
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '8538fc5b-e6fa-4924-a0d4-dcc47ebaa421'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:28 | RSV:Adult  | 7           | 1        |
      | Tomorrow          | 09:00 | 09:30 | COVID:5_11 | 3           | 1        |
    When I query availability by slots
      | Site                                 | Attendee Services    | Date     | From  | Until |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | RSV:Adult,COVID:5_11 | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00'
      | From  | Until | Services   |
      | 09:18 | 09:21 | COVID:5_11 |
      | 09:21 | 09:28 | RSV:Adult  |
      | 09:14 | 09:21 | RSV:Adult  |
      | 09:21 | 09:24 | COVID:5_11 |

  Scenario: Returns slots that are just outside the hour range if they are suitable for a joint booking
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '8538fc5b-e6fa-4924-a0d4-dcc47ebaa421'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 08:50 | 10:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 09:00 | 10:10 | COVID:5_11 | 10          | 1        |
    When I query availability by slots
      | Site                                 | Attendee Services    | Date     | From  | Until |
      | 8538fc5b-e6fa-4924-a0d4-dcc47ebaa421 | RSV:Adult,COVID:5_11 | Tomorrow | 09:00 | 10:00 |
    Then the following availability is returned for 'Tomorrow' between '09:00' and '10:00'
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
