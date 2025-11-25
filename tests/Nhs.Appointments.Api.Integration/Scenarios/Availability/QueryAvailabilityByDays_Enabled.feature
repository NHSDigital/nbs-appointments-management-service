Feature: Query Availability By Days

  Scenario: Only returns matching days for a single attendee at a single site with a single service
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 41b5fc18-0115-4f84-a780-af5a3025c6fe | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '41b5fc18-0115-4f84-a780-af5a3025c6fe'
      | Date              | From  | Until | Attendee Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I query availability by days
      | Site                                 | Service   | From     | Until             |
      | 41b5fc18-0115-4f84-a780-af5a3025c6fe | RSV:Adult | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |
      | Tomorrow          | AM,PM  | 09:00 | 17:00 |
      | 2 days from today | PM     | 12:00 | 17:00 |

  Scenario: Only returns matching days for rwo Attendees with two Services at a single Site
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 94a19f36-2a34-4921-89e4-a3b95083c362 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '94a19f36-2a34-4921-89e4-a3b95083c362'
      | Date              | From  | Until | Attendee Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | COVID:5_11 | 10          | 1        |
    When I query availability by days
      | Site                                 | Attendee Services             | From     | Until             |
      | 94a19f36-2a34-4921-89e4-a3b95083c362 | RSV:Adult,COVID:5_11 | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |
      | 2 days from today | PM     | 12:00 | 17:00 |

  Scenario: Only returns matching days for two attendees with a single service at a single Site
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 3284fc27-3eb4-4e01-b49a-115a74d0958b | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '3284fc27-3eb4-4e01-b49a-115a74d0958b'
      | Date              | From  | Until | Attendee Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | COVID:5_11 | 10          | 1        |
    When I query availability by days
      | Site                                 | Attendee Services            | From     | Until             |
      | 3284fc27-3eb4-4e01-b49a-115a74d0958b | RSV:Adult,RSV:Adult | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |
      | Tomorrow          | AM,PM  | 09:00 | 17:00 |
      | 2 days from today | PM     | 12:00 | 17:00 |

  Scenario: Only return matching slots for three attendees requesting three services at a single site
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 8818249b-e654-46d5-9b50-607c28996abe | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '8818249b-e654-46d5-9b50-607c28996abe'
      | Date              | From  | Until | Attendee Services   | Slot Length | Capacity |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 09:00 | 17:00 | FLU:18_64  | 10          | 1        |
      | 2 days from today | 09:00 | 17:00 | COVID:5_11 | 10          | 1        |
    When I query availability by days
      | Site                                 | Attendee Services                       | From     | Until             |
      | 8818249b-e654-46d5-9b50-607c28996abe | RSV:Adult,COVID:5_11,FLU:18_64 | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |
      | 2 days from today | AM,PM  | 09:00 | 17:00 |

  Scenario: Only return matching days for three attendees requesting two services at a single site
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 9fdd5102-49dc-49d3-a9b1-52a447ebd160 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '9fdd5102-49dc-49d3-a9b1-52a447ebd160'
      | Date              | From  | Until | Attendee Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | FLU:18_64  | 10          | 1        |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 09:00 | 17:00 | COVID:5_11 | 10          | 1        |
    When I query availability by days
      | Site                                 | Attendee Services                       | From     | Until             |
      | 9fdd5102-49dc-49d3-a9b1-52a447ebd160 | RSV:Adult,COVID:5_11,RSV:Adult | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |
      | 2 days from today | AM,PM  | 09:00 | 17:00 |

  Scenario: Only return matching days for three attendees requesting a single service at a single site
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 9ad300b4-7608-497b-b0e2-0a54192dabd4 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '9ad300b4-7608-497b-b0e2-0a54192dabd4'
      | Date              | From  | Until | Attendee Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | FLU:18_64  | 10          | 1        |
      | 2 days from today | 09:00 | 17:00 | RSV:Adult  | 10          | 1        |
    When I query availability by days
      | Site                                 | Attendee Services                      | From     | Until             |
      | 9ad300b4-7608-497b-b0e2-0a54192dabd4 | RSV:Adult,RSV:Adult,RSV:Adult          | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |
      | 2 days from today | AM,PM  | 09:00 | 17:00 |

  Scenario: Return matching days for a single attendee at multiple sites
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | aa0ffcae-89dd-4017-8eda-c5c0dbdcbe07 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
      | 0a377442-1be4-413c-ac36-a2583864704b | Site-B | 1B Site Lane | 0113 1111112 | 15P     | R2     | ICB2 | Info 2                 | accessibility/attr_one=true  | -60       | -60      | Pharmacy    |
    And the following sessions exist for site 'aa0ffcae-89dd-4017-8eda-c5c0dbdcbe07'
      | Date              | From  | Until | Attendee Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following sessions exist for site '0a377442-1be4-413c-ac36-a2583864704b'
      | Date              | From  | Until | Attendee Services  | Slot Length | Capacity |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult          | 10          | 1        |
    When I query availability by days
      | Sites                                                                      | Service   | From     | Until             |
      | aa0ffcae-89dd-4017-8eda-c5c0dbdcbe07,0a377442-1be4-413c-ac36-a2583864704b  | RSV:Adult | Tomorrow | 2 days from today |
    Then the following multi-site availability is returned
      | Site                                 | Date              | Blocks | From  | Until |
      | aa0ffcae-89dd-4017-8eda-c5c0dbdcbe07 | Tomorrow          | AM,PM  | 09:00 | 17:00 |
      | 0a377442-1be4-413c-ac36-a2583864704b | 2 days from today | PM     | 09:00 | 17:00 |

  Scenario: Return matching days for two attendees at multiple sites for a single service
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 252b0be3-cb39-443e-95a9-06ac37c63346 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
      | b17d91f0-4332-4e66-83a9-2978b7e36456 | Site-B | 1B Site Lane | 0113 1111112 | 15P     | R2     | ICB2 | Info 2                 | accessibility/attr_one=true  | -60       | -60      | Pharmacy    |
    And the following sessions exist for site '252b0be3-cb39-443e-95a9-06ac37c63346'
      | Date              | From  | Until | Attendee Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following sessions exist for site 'b17d91f0-4332-4e66-83a9-2978b7e36456'
      | Date              | From  | Until | Attendee Services  | Slot Length | Capacity |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I query availability by days
      | Sites                                                                      | Service             | From     | Until             |
      | 252b0be3-cb39-443e-95a9-06ac37c63346,b17d91f0-4332-4e66-83a9-2978b7e36456  | RSV:Adult,RSV:Adult | Tomorrow | 2 days from today |
    Then the following multi-site availability is returned
      | Site                                 | Date              | Blocks | From  | Until |
      | 252b0be3-cb39-443e-95a9-06ac37c63346 | Tomorrow          | AM,PM  | 09:00 | 17:00 |
      | b17d91f0-4332-4e66-83a9-2978b7e36456 | 2 days from today | PM     | 09:00 | 17:00 |

  Scenario: Return matching days for two attendees at multiple sits for multiple services
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 41708fe0-6c89-46a4-8183-1a8d0f919cc2 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
      | 1ac0fd0e-c421-4633-bc23-95dc92003803 | Site-B | 1B Site Lane | 0113 1111112 | 15P     | R2     | ICB2 | Info 2                 | accessibility/attr_one=true  | -60       | -60      | Pharmacy    |
    And the following sessions exist for site '41708fe0-6c89-46a4-8183-1a8d0f919cc2'
      | Date              | From  | Until | Attendee Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following sessions exist for site '1ac0fd0e-c421-4633-bc23-95dc92003803'
      | Date              | From  | Until | Attendee Services   | Slot Length | Capacity |
      | 2 days from today | 12:00 | 17:00 | COVID:5_11 | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult  | 10          | 1        |
    When I query availability by days
      | Sites                                                                      | Service              | From     | Until             |
      | 41708fe0-6c89-46a4-8183-1a8d0f919cc2,1ac0fd0e-c421-4633-bc23-95dc92003803  | RSV:Adult,COVID:5_11 | Tomorrow | 2 days from today |
    Then the following multi-site availability is returned
      | Site                                 | Date              | Blocks | From  | Until |
      | 1ac0fd0e-c421-4633-bc23-95dc92003803 | 2 days from today | PM     | 09:00 | 17:00 |
      | 41708fe0-6c89-46a4-8183-1a8d0f919cc2 | 2 days from today |        |       |       |

  Scenario: Returns empty days array when there are no matching slots
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 2debe217-0cc1-4b53-8a2a-56315ca4528a | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '2debe217-0cc1-4b53-8a2a-56315ca4528a'
      | Date              | From  | Until | Attendee Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I query availability by days
      | Site                                 | Service    | From     | Until             |
      | 2debe217-0cc1-4b53-8a2a-56315ca4528a | COVID:5_11 | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |

  Scenario: Returns empty days array when there are no matching slots for multiple attendees requesting multiple services even though one service matches
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'b4f1093b-83fc-4f99-9bd2-7c29080254db'
      | Date              | From  | Until | Attendee Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 09:00 | 17:00 | COVID:5_11 | 10          | 1        |
    When I query availability by days
      | Site                                 | Attendee Services          | From     | Until             |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | RSV:Adult,FLU:2_3 | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |

  Scenario: Returns Bad Request Response
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | b2b3ee1c-3cc3-4903-b3c7-6cdd23c4b799 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'b2b3ee1c-3cc3-4903-b3c7-6cdd23c4b799'
      | Date              | From  | Until | Attendee Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I pass an invalid payload
    Then the call should fail with 400

  Scenario: Returns empty array when sites are inactive
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Type         | IsDeleted |
      | 5fc5b424-07a4-496b-96c6-500a68554435 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  | true      |
      | 9e53e798-9b60-442f-b62c-aded0290ae47 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  | true      |
    When I query availability by days
      | Site                                 | Service    | From     | Until             |
      | 5fc5b424-07a4-496b-96c6-500a68554435 | COVID:5_11 | Tomorrow | 2 days from today |
    Then the response should be empty

  Scenario: Returns matching slots of different lengths for two attendees requesting a single service
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Type         |
      | 6188c242-acfa-4dc2-860a-ead658bfe180 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  |
    And the following sessions exist for site '6188c242-acfa-4dc2-860a-ead658bfe180'
      | Date              | From  | Until | Attendee Services  | Slot Length | Capacity |
      | Tomorrow          | 11:00 | 12:00 | RSV:Adult | 10          | 1        |
      | Tomorrow          | 12:00 | 13:00 | RSV:Adult | 5           | 1        |
    When I query availability by days
      | Site                                 | Service             | From     | Until             |
      | 6188c242-acfa-4dc2-860a-ead658bfe180 | RSV:Adult,RSV:Adult | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM,PM  | 11:00 | 13:00 |

  Scenario: Returns matching slots of different lengths for two attendees requesting multiple services
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Type         |
      | 091ea500-4b8c-49f1-93cc-ac1fb25f18d3 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  |
    And the following sessions exist for site '091ea500-4b8c-49f1-93cc-ac1fb25f18d3'
      | Date              | From  | Until | Attendee Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 12:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 12:00 | 14:00 | COVID:5_11 | 5           | 1        |
    When I query availability by days
      | Site                                 | Service              | From     | Until             |
      | 091ea500-4b8c-49f1-93cc-ac1fb25f18d3 | RSV:Adult,COVID:5_11 | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM,PM  | 11:50 | 12:05 |
