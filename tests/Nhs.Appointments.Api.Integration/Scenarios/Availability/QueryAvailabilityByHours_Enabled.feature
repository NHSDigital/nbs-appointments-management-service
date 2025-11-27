Feature: Query Availability By Hours

  Scenario: Returns 404 when site can't be found
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 41b5fc18-0115-4f84-a780-af5a3025c6fe | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '41b5fc18-0115-4f84-a780-af5a3025c6fe'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I query availability by hours
      | Site                                 | Attendee Services   | From     | Until             |
      | e8f0c55a-571b-4538-a073-a626cca53547 | RSV:Adult           | Tomorrow | 2 days from today |
    Then the call should fail with 404

  Scenario: Returns bad request when payload is invalid
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 41b5fc18-0115-4f84-a780-af5a3025c6fe | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '41b5fc18-0115-4f84-a780-af5a3025c6fe'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I pass an invalid payload
    Then the call should fail with 400

  Scenario: Returns bad request when too many attendees passed up
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 41b5fc18-0115-4f84-a780-af5a3025c6fe | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '41b5fc18-0115-4f84-a780-af5a3025c6fe'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I query availability by hours
      | Site                                 | Attendee Services                                           | From     | Until             |
      | e8f0c55a-571b-4538-a073-a626cca53547 | RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow | 2 days from today |
    Then the call should fail with 400

  Scenario: Can return availability for single attendee
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 41b5fc18-0115-4f84-a780-af5a3025c6fe | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '41b5fc18-0115-4f84-a780-af5a3025c6fe'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    When I query availability by hours
      | Site                                 | Attendee Services | From     | Until             |
      | 41b5fc18-0115-4f84-a780-af5a3025c6fe | RSV:Adult         | Tomorrow | 2 days from today |
    Then the following 'RSV:Adult' availabilty is returned for 'Tomorrow'
      | From  | Until |
      | 09:00 | 10:00 |
      | 10:00 | 11:00 |
      | 11:00 | 12:00 |

  Scenario: Can return availability for single attendee over multiple sessions
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 7711f16b-24e0-4da0-b8f2-690d76756409 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '7711f16b-24e0-4da0-b8f2-690d76756409'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
      | Tomorrow          | 14:00 | 16:00 | RSV:Adult | 10          | 1        |
    When I query availability by hours
      | Site                                 | Attendee Services | From     | Until    |
      | 7711f16b-24e0-4da0-b8f2-690d76756409 | RSV:Adult         | Tomorrow | Tomorrow |
    Then the following 'RSV:Adult' availabilty is returned for 'Tomorrow'
      | From  | Until |
      | 09:00 | 10:00 |
      | 10:00 | 11:00 |
      | 11:00 | 12:00 |
      | 14:00 | 15:00 |
      | 15:00 | 16:00 |

  Scenario: Returns empty hours array when there is no availability for requested service
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | d18229ed-a4c2-409e-947a-409e1a860595 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'd18229ed-a4c2-409e-947a-409e1a860595'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 12:00 | RSV:Adult | 10          | 1        |
    When I query availability by hours
      | Site                                 | Attendee Services | From     | Until    |
      | d18229ed-a4c2-409e-947a-409e1a860595 | COVID:5_11        | Tomorrow | Tomorrow |
    Then the following 'RSV:Adult' availabilty is returned for 'Tomorrow'
      | From  | Until |

  Scenario: Correctly displays midnight time
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 05a9d6f1-c6df-4b06-9490-90a6c05a0154 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '05a9d6f1-c6df-4b06-9490-90a6c05a0154'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 21:00 | 23:30 | RSV:Adult | 10          | 1        |
    When I query availability by hours
      | Site                                 | Attendee Services | From     | Until    |
      | 05a9d6f1-c6df-4b06-9490-90a6c05a0154 | RSV:Adult         | Tomorrow | Tomorrow |
    Then the following 'RSV:Adult' availabilty is returned for 'Tomorrow'
      | From  | Until |
      | 21:00 | 22:00 |
      | 22:00 | 23:00 |
      | 23:00 | 00:00 |

  Scenario: Only returns hour blocks from start time of slot and doesn't include spillover hour
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | a2beb3ad-9cc5-4b8e-bd40-e7573d309a30 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'a2beb3ad-9cc5-4b8e-bd40-e7573d309a30'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 10:55 | 11:05 | RSV:Adult | 10          | 1        |
    When I query availability by hours
      | Site                                 | Attendee Services | From     | Until    |
      | a2beb3ad-9cc5-4b8e-bd40-e7573d309a30 | RSV:Adult         | Tomorrow | Tomorrow |
    Then the following 'RSV:Adult' availabilty is returned for 'Tomorrow'
      | From  | Until |
      | 10:00 | 11:00 |

  Scenario: Returns correct hour blocks for two attendees requesting a single service
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | fb124a92-4c0d-4fe7-b8f3-2166a9f49444 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'fb124a92-4c0d-4fe7-b8f3-2166a9f49444'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 13:00 | RSV:Adult | 10          | 1        |
    When I query availability by hours
      | Site                                 | Attendee Services   | From     | Until    |
      | fb124a92-4c0d-4fe7-b8f3-2166a9f49444 | RSV:Adult,RSV:Adult | Tomorrow | Tomorrow |
    Then the following 'RSV:Adult' availabilty is returned for 'Tomorrow'
      | From  | Until |
      | 10:00 | 11:00 |
      | 11:00 | 12:00 |
      | 12:00 | 13:00 |

  Scenario: Returns empty hours array when there aren't enough slots for multiple attendees
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 4321f15b-7931-4af1-8c1d-c2e105c64ed5 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '4321f15b-7931-4af1-8c1d-c2e105c64ed5'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 11:00 | RSV:Adult | 15          | 1        |
    When I query availability by hours
      | Site                                 | Attendee Services                                 | From     | Until    |
      | 4321f15b-7931-4af1-8c1d-c2e105c64ed5 | RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow | Tomorrow |
    Then the following 'RSV:Adult' availabilty is returned for 'Tomorrow'
      | From  | Until |

  Scenario: Correctly returns hour blocks for two attendees requesting two services
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 13154d5b-b1eb-4647-a562-fcaedf923e93 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '13154d5b-b1eb-4647-a562-fcaedf923e93'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 13:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 11:00 | 13:00 | COVID:5_11 | 10          | 1        |
    When I query availability by hours
      | Site                                 | Attendee Services    | From     | Until    |
      | 13154d5b-b1eb-4647-a562-fcaedf923e93 | RSV:Adult,COVID:5_11 | Tomorrow | Tomorrow |
    Then the following 'RSV:Adult' availabilty is returned for 'Tomorrow'
      | From  | Until |
      | 10:00 | 11:00 |
      | 11:00 | 12:00 |
      | 12:00 | 13:00 |

  Scenario: Returns matching slots of different lengths for two attendees request two services
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 4efefc0f-d5a1-40b2-88bb-482b8d82e9d9 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '4efefc0f-d5a1-40b2-88bb-482b8d82e9d9'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 13:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 12:00 | 13:00 | COVID:5_11 | 5           | 1        |
    When I query availability by hours
      | Site                                 | Attendee Services    | From     | Until    |
      | 4efefc0f-d5a1-40b2-88bb-482b8d82e9d9 | RSV:Adult,COVID:5_11 | Tomorrow | Tomorrow |
    Then the following 'RSV:Adult' availabilty is returned for 'Tomorrow'
      | From  | Until |
      | 11:00 | 12:00 |
      | 12:00 | 13:00 |

  Scenario: Just enough availability for the amount of attendees requested
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | d6f3a47e-9f77-4866-9e99-9d3a6b56fa6f | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'd6f3a47e-9f77-4866-9e99-9d3a6b56fa6f'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 11:00 | RSV:Adult  | 15          | 1        |
    When I query availability by hours
      | Site                                 | Attendee Services                       | From     | Until    |
      | d6f3a47e-9f77-4866-9e99-9d3a6b56fa6f | RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow | Tomorrow |
    Then the following 'RSV:Adult' availabilty is returned for 'Tomorrow'
      | From  | Until |
      | 10:00 | 11:00 |

  Scenario: Returns empty hours array when neighbouring slots don't exactly match up
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | d83f2bd8-bde4-452b-8368-e74f84467a71 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'd83f2bd8-bde4-452b-8368-e74f84467a71'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 13:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 13:01 | 14:00 | COVID:5_11 | 5           | 1        |
    When I query availability by hours
      | Site                                 | Attendee Services    | From     | Until    |
      | d83f2bd8-bde4-452b-8368-e74f84467a71 | RSV:Adult,COVID:5_11 | Tomorrow | Tomorrow |
    Then the following 'RSV:Adult' availabilty is returned for 'Tomorrow'
      | From  | Until |

  Scenario: Returns availability for 3 attendees requesting a single service
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 1287dedd-ff45-42f2-96a7-8009b2feff15 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '1287dedd-ff45-42f2-96a7-8009b2feff15'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 13:00 | RSV:Adult  | 10          | 1        |
    When I query availability by hours
      | Site                                 | Attendee Services             | From     | Until    |
      | 1287dedd-ff45-42f2-96a7-8009b2feff15 | RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow | Tomorrow |
    Then the following 'RSV:Adult' availabilty is returned for 'Tomorrow'
      | From  | Until |
      | 10:00 | 11:00 |
      | 11:00 | 12:00 |
      | 12:00 | 13:00 |

  Scenario: Returns availability for 3 attendees requesting two services
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | e83667a2-a1f2-414b-9490-807052f869dc | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'e83667a2-a1f2-414b-9490-807052f869dc'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 13:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 12:00 | 14:00 | COVID:5_11 | 5           | 1        |
    When I query availability by hours
      | Site                                 | Attendee Services              | From     | Until    |
      | e83667a2-a1f2-414b-9490-807052f869dc | RSV:Adult,COVID:5_11,RSV:Adult | Tomorrow | Tomorrow |
    Then the following 'RSV:Adult' availabilty is returned for 'Tomorrow'
      | From  | Until |
      | 11:00 | 12:00 |
      | 12:00 | 13:00 |
      | 13:00 | 14:00 |

  Scenario: Returns availability for 3 attendees requesting three services
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | b2a620b7-c39a-47dc-97e9-ea6ac3216ce7 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'b2a620b7-c39a-47dc-97e9-ea6ac3216ce7'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 13:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 12:00 | 14:00 | COVID:5_11 | 5           | 1        |
      | Tomorrow          | 11:00 | 14:00 | FLU:2_3    | 5           | 1        |
    When I query availability by hours
      | Site                                 | Attendee Services            | From     | Until    |
      | b2a620b7-c39a-47dc-97e9-ea6ac3216ce7 | RSV:Adult,COVID:5_11,FLU:2_3 | Tomorrow | Tomorrow |
    Then the following 'RSV:Adult' availabilty is returned for 'Tomorrow'
      | From  | Until |
      | 11:00 | 12:00 |
      | 12:00 | 13:00 |
      | 13:00 | 14:00 |
