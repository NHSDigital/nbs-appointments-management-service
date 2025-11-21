Feature: Query Availability By Days

  Scenario: Single Attendee - Single Service - Single Site
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 41b5fc18-0115-4f84-a780-af5a3025c6fe | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '41b5fc18-0115-4f84-a780-af5a3025c6fe'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I query availability for a single attendee
      | Site                                 | Service   | From     | Until             |
      | 41b5fc18-0115-4f84-a780-af5a3025c6fe | RSV:Adult | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |
      | Tomorrow          | AM,PM  | 09:00 | 17:00 |
      | 2 days from today | PM     | 12:00 | 17:00 |

  Scenario: Two Attendees - Two Services - Single Site
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 94a19f36-2a34-4921-89e4-a3b95083c362 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '94a19f36-2a34-4921-89e4-a3b95083c362'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | COVID:5_11 | 10          | 1        |
    When I query availability for two attendees
      | Site                                 | Services             | From     | Until             |
      | 94a19f36-2a34-4921-89e4-a3b95083c362 | RSV:Adult,COVID:5_11 | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |
      | 2 days from today | PM     | 12:00 | 17:00 |

  Scenario: Two Attendees - Single Service - Single Site
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 3284fc27-3eb4-4e01-b49a-115a74d0958b | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '3284fc27-3eb4-4e01-b49a-115a74d0958b'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | COVID:5_11 | 10          | 1        |
    When I query availability for two attendees
      | Site                                 | Services            | From     | Until             |
      | 3284fc27-3eb4-4e01-b49a-115a74d0958b | RSV:Adult,RSV:Adult | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |
      | Tomorrow          | AM,PM  | 09:00 | 17:00 |
      | 2 days from today | PM     | 12:00 | 17:00 |

  Scenario: Three Attendees - Three Services - Single Site
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 8818249b-e654-46d5-9b50-607c28996abe | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '8818249b-e654-46d5-9b50-607c28996abe'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 09:00 | 17:00 | FLU:18_64  | 10          | 1        |
      | 2 days from today | 09:00 | 17:00 | COVID:5_11 | 10          | 1        |
    When I query availability for three attendees
      | Site                                 | Services                       | From     | Until             |
      | 8818249b-e654-46d5-9b50-607c28996abe | RSV:Adult,COVID:5_11,FLU:18_64 | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |
      | 2 days from today | AM,PM  | 09:00 | 17:00 |

  Scenario: Three Attendees - Two Services - Single Site
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 9fdd5102-49dc-49d3-a9b1-52a447ebd160 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '9fdd5102-49dc-49d3-a9b1-52a447ebd160'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | FLU:18_64  | 10          | 1        |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 09:00 | 17:00 | COVID:5_11 | 10          | 1        |
    When I query availability for three attendees
      | Site                                 | Services                       | From     | Until             |
      | 9fdd5102-49dc-49d3-a9b1-52a447ebd160 | RSV:Adult,COVID:5_11,RSV:Adult | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |
      | 2 days from today | AM,PM  | 09:00 | 17:00 |

  Scenario: Three Attendees - Single Service - Single Site
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 9ad300b4-7608-497b-b0e2-0a54192dabd4 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '9ad300b4-7608-497b-b0e2-0a54192dabd4'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | FLU:18_64  | 10          | 1        |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult  | 10          | 1        |
    When I query availability for three attendees
      | Site                                 | Services                      | From     | Until             |
      | 9ad300b4-7608-497b-b0e2-0a54192dabd4 | RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |
      | 2 days from today | AM,PM  | 09:00 | 17:00 |

  Scenario: Single Attendee - Multi Site
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | aa0ffcae-89dd-4017-8eda-c5c0dbdcbe07 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
      | 0a377442-1be4-413c-ac36-a2583864704b | Site-B | 1B Site Lane | 0113 1111112 | 15P     | R2     | ICB2 | Info 2                 | accessibility/attr_one=true  | -60       | -60      | Pharmacy    |
    And the following sessions exist for site 'aa0ffcae-89dd-4017-8eda-c5c0dbdcbe07'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following sessions exist for site '0a377442-1be4-413c-ac36-a2583864704b'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I query availability for a single attendee across multiple sites
      | Sites                                                                      | Service   | From     | Until             |
      | aa0ffcae-89dd-4017-8eda-c5c0dbdcbe07,0a377442-1be4-413c-ac36-a2583864704b  | RSV:Adult | Tomorrow | 2 days from today |
    Then the following multi-site availability is returned
      | Site                                 | Date              | Blocks | From  | Until |
      | aa0ffcae-89dd-4017-8eda-c5c0dbdcbe07 | Tomorrow          | AM,PM  | 09:00 | 17:00 |
      | 0a377442-1be4-413c-ac36-a2583864704b | 2 days from today | PM     | 09:00 | 17:00 |
