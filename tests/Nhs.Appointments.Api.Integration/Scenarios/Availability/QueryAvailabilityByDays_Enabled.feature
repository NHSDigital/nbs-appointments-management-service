Feature: Query Availability By Days

  Scenario: Only returns matching days for a single attendee at a single site with a single service
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 41b5fc18-0115-4f84-a780-af5a3025c6fe | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '41b5fc18-0115-4f84-a780-af5a3025c6fe'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I query availability by days
      | Site                                 | Attendee Services   | From     | Until             |
      | 41b5fc18-0115-4f84-a780-af5a3025c6fe | RSV:Adult           | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |
      | Tomorrow          | AM,PM  | 09:00 | 17:00 |
      | 2 days from today | PM     | 12:00 | 17:00 |

    Scenario: Can return availability if it crosses multiple sessions
      Given the following sessions exist for site '41b5fc18-0115-4f84-a780-af5a3025c6fa'
        | Date              | From  | Until | Services  | Slot Length | Capacity |
        | Tomorrow          | 09:00 | 16:50 | RSV:Adult | 10          | 1        |
        | Tomorrow          | 16:50 | 17:00 | RSV:Adult | 10          | 1        |
      When I query availability by days
        | Site                                 | Attendee Services   | From     | Until    |
        | 41b5fc18-0115-4f84-a780-af5a3025c6fa | RSV:Adult           | Tomorrow | Tomorrow |
      Then the following single site availability by days is returned
        | Date              | Blocks | From  | Until |
        | Tomorrow          | AM,PM  | 09:00 | 17:00 |

    Scenario: 'Neighbouring' slot logic is not ideal if slot lengths do not easily divide across services - no results
      Given the following sessions exist for site '41b5fc18-0115-4f84-a780-af5a3025c6fb'
        | Date              | From  | Until | Services   | Slot Length | Capacity |
        | Tomorrow          | 09:00 | 09:21 | RSV:Adult  | 7          | 1        |
        | Tomorrow          | 09:00 | 09:18 | COVID:5_11 | 3          | 1        |
      When I query availability by days
        | Site                                 | Attendee Services      | From     | Until    |
        | 41b5fc18-0115-4f84-a780-af5a3025c6fb | RSV:Adult,COVID:5_11   | Tomorrow | Tomorrow |
      # Ideally in future we might return the 9:07-9:14 slot for RSV, and the 9:15-9:18 slot for Covid as they are only a minute apart!
      Then the response should be empty

    Scenario: 'Neighbouring' slot logic is not ideal if slot lengths do not easily divide across services - single result
      Given the following sessions exist for site '41b5fc18-0115-4f84-a780-af5a3025c6fe'
        | Date              | From  | Until | Services   | Slot Length | Capacity |
        | Tomorrow          | 09:00 | 09:28 | RSV:Adult  | 7           | 1        |
        | Tomorrow          | 09:00 | 09:30 | COVID:5_11 | 3           | 1        |
      When I query availability by days
        | Site                                 | Attendee Services      | From     | Until    |
        | 41b5fc18-0115-4f84-a780-af5a3025c6fe | RSV:Adult,COVID:5_11   | Tomorrow | Tomorrow |
      # The 9:18-9:21 slot (Covid) and the 9:21-9:28 slot (RSV) are neighboring - as are the 09:14-09:21 (RSV) and the 09:21-09:28 (COVID)
      Then the following single site availability by days is returned
        | Date              | Blocks | From  | Until |
        | Tomorrow          | AM     | 09:14 | 12:00 |

  Scenario: Only returns matching days for rwo Attendees with two Services at a single Site
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 94a19f36-2a34-4921-89e4-a3b95083c362 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '94a19f36-2a34-4921-89e4-a3b95083c362'
      | Date              | From  | Until | Service    | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult  | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | COVID:5_11 | 10          | 1        |
    When I query availability by days
      | Site                                 | Attendee Services             | From     | Until             |
      | 94a19f36-2a34-4921-89e4-a3b95083c362 | RSV:Adult,COVID:5_11          | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |
      | 2 days from today | PM     | 12:00 | 17:00 |

  Scenario: Only returns matching days for two attendees with a single service at a single Site
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 3284fc27-3eb4-4e01-b49a-115a74d0958b | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '3284fc27-3eb4-4e01-b49a-115a74d0958b'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
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
      | Date              | From  | Until | Services   | Slot Length | Capacity |
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
      | Date              | From  | Until | Services   | Slot Length | Capacity |
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
      | Date              | From  | Until | Services   | Slot Length | Capacity |
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
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following sessions exist for site '0a377442-1be4-413c-ac36-a2583864704b'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult          | 10          | 1        |
    When I query availability by days
      | Sites                                                                      | Attendee Services   | From     | Until             |
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
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following sessions exist for site 'b17d91f0-4332-4e66-83a9-2978b7e36456'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I query availability by days
      | Sites                                                                      | Attendee Services             | From     | Until             |
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
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    And the following sessions exist for site '1ac0fd0e-c421-4633-bc23-95dc92003803'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | 2 days from today | 12:00 | 17:00 | COVID:5_11 | 10          | 1        |
      | 2 days from today | 12:00 | 17:00 | RSV:Adult  | 10          | 1        |
    When I query availability by days
      | Sites                                                                      | Attendee Services              | From     | Until             |
      | 41708fe0-6c89-46a4-8183-1a8d0f919cc2,1ac0fd0e-c421-4633-bc23-95dc92003803  | RSV:Adult,COVID:5_11           | Tomorrow | 2 days from today |
    Then the following multi-site availability is returned
      | Site                                 | Date              | Blocks | From  | Until |
      | 1ac0fd0e-c421-4633-bc23-95dc92003803 | 2 days from today | PM     | 09:00 | 17:00 |
      | 41708fe0-6c89-46a4-8183-1a8d0f919cc2 | 2 days from today |        |       |       |

  Scenario: Returns empty days array when there are no matching slots
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | 2debe217-0cc1-4b53-8a2a-56315ca4528a | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site '2debe217-0cc1-4b53-8a2a-56315ca4528a'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I query availability by days
      | Site                                 | Attendee Services    | From     | Until             |
      | 2debe217-0cc1-4b53-8a2a-56315ca4528a | COVID:5_11           | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |

  Scenario: Returns empty days array when there are no matching slots for multiple attendees requesting multiple services even though one service matches
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'b4f1093b-83fc-4f99-9bd2-7c29080254db'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 09:00 | 17:00 | COVID:5_11 | 10          | 1        |
    When I query availability by days
      | Site                                 | Attendee Services    | From     | Until             |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | RSV:Adult,FLU:2_3    | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |

  Scenario: Returns Bad Request Response
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | b2b3ee1c-3cc3-4903-b3c7-6cdd23c4b799 | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'b2b3ee1c-3cc3-4903-b3c7-6cdd23c4b799'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 17:00 | RSV:Adult | 10          | 1        |
    When I pass an invalid payload
    Then the call should fail with 400

  Scenario: Returns empty array when sites are inactive
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Type         | IsDeleted |
      | 5fc5b424-07a4-496b-96c6-500a68554435 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  | true      |
      | 9e53e798-9b60-442f-b62c-aded0290ae47 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  | true      |
    When I query availability by days
      | Site                                 | Attendee Services    | From     | Until             |
      | 5fc5b424-07a4-496b-96c6-500a68554435 | COVID:5_11           | Tomorrow | 2 days from today |
    Then the response should be empty

  Scenario: Returns matching slots of different lengths for two attendees requesting a single service
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Type         |
      | 6188c242-acfa-4dc2-860a-ead658bfe180 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  |
    And the following sessions exist for site '6188c242-acfa-4dc2-860a-ead658bfe180'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 11:00 | 12:00 | RSV:Adult | 10          | 1        |
      | Tomorrow          | 12:00 | 13:00 | RSV:Adult | 5           | 1        |
    When I query availability by days
      | Site                                 | Attendee Services   | From     | Until             |
      | 6188c242-acfa-4dc2-860a-ead658bfe180 | RSV:Adult,RSV:Adult | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM,PM  | 11:00 | 13:00 |

  Scenario: Not enough availability for the amount of attendees required
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Type         |
      | 6188c242-acfa-4dc2-860a-ead658bfe180 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  |
    And the following sessions exist for site '6188c242-acfa-4dc2-860a-ead658bfe184'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 11:00 | 12:00 | RSV:Adult | 15          | 1        |
    When I query availability by days
      | Site                                 | Attendee Services                                  | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe184 | RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult  | Tomorrow | Tomorrow |
    # 5 attendees can't fit within the single hour (4 slots)
    Then the response should be empty

  Scenario: Just enough availability for the amount of attendees required
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Type         |
      | 6188c242-acfa-4dc2-860a-ead658bfe180 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  |
    And the following sessions exist for site '6188c242-acfa-4dc2-860a-ead658bfe185'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 11:00 | 12:00 | RSV:Adult | 15          | 1        |
    When I query availability by days
      | Site                                 | Attendee Services                       | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe185 | RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow | Tomorrow |
    # 4 attendees can just fit within the single hour (4 slots)
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM     | 11:00 | 12:00 |

  Scenario: Returns matching slots of different lengths for two attendees requesting multiple services
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Type         |
      | 091ea500-4b8c-49f1-93cc-ac1fb25f18d3 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  |
    And the following sessions exist for site '091ea500-4b8c-49f1-93cc-ac1fb25f18d3'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 10:00 | 12:00 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 12:00 | 14:00 | COVID:5_11 | 5           | 1        |
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until             |
      | 091ea500-4b8c-49f1-93cc-ac1fb25f18d3 | RSV:Adult,COVID:5_11  | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM,PM  | 11:50 | 12:05 |

  Scenario: Returns bad request when too many attendees are passed up
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Type         |
      | 6188c242-acfa-4dc2-860a-ead658bfe185 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  |
    And the following sessions exist for site '6188c242-acfa-4dc2-860a-ead658bfe185'
      | Date              | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow          | 11:00 | 12:00 | RSV:Adult | 15          | 1        |
    When I query availability by days
      | Site                                 | Attendee Services                                           | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe185 | RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult,RSV:Adult | Tomorrow | Tomorrow |
    Then the call should fail with 400
    
  # Due to service length allocation logic, more overall JB combinations are available to the APIs vs a 'first session found' allocation
  # However, it is not optimal, as some pairings are not offered, when they COULD be in a best fit solution
  # All 12 combinations of services from sessions A and B should be available
  # All 6 combinations of services that include 'FLU:2_3' from session C and other services from A/B - however - are unavailable, but they COULD be in best fit solution...
  Scenario: Greedy allocation by service length - optimal and suboptimal combinations
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Type         |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | GP Practice  |
    And the following sessions exist for site '6188c242-acfa-4dc2-860a-ead658bfe187'
      | Date        | From  | Until | Services                               | Slot Length  | Capacity |
      # Session 'A'
      | Tomorrow    | 09:00 | 09:30 | COVID, RSV:Adult, FLU:5_11             | 15           | 1        |
      # Session 'B'
      | Tomorrow    | 09:00 | 09:30 | COVID, FLU:64_80, FLU:11_18, FLU:18:40 | 15           | 1        |
      # Session 'C'
      | Tomorrow    | 09:00 | 09:30 | COVID, FLU:2_3                         | 15           | 1        |
    And the following bookings have been made for site '6188c242-acfa-4dc2-860a-ead658bfe187'
      | Date        | Time  | Duration  | Service |
      | Tomorrow    | 09:00 | 15        | COVID   |
      | Tomorrow    | 09:15 | 15        | COVID   |
    # OPTIMAL
    # In a 'first fit' algorithm, the two COVID bookings would both be allocated to Session 'A' (the first)
    # This would remove 'FLU:5_11' availability, making it unavailable to the JB response
    # Fortunately - the greedy algorithm assigns both bookings to session 'C', allowing this combination
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | FLU:64_80, FLU:5_11   | Tomorrow | Tomorrow |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM     | 09:00 | 09:30 |
    # -- END OPTIMAL
    # SUBOPTIMAL
    # In a best fit solution, the two COVID bookings would both be allocated to Session 'B' (the longest)
    # This would free up both Sessions 'A' and 'C', to allow availability for the JB requested
    # Unfortunately - the greedy algorithm assigns them both to session 'C', denying this JB request
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | FLU:2_3, FLU:5_11     | Tomorrow | Tomorrow |
    Then the response should be empty
    # -- END SUBOPTIMAL
    # OPTIMAL
    # Other available combinations
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | COVID, FLU:5_11       | Tomorrow | Tomorrow |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM     | 09:00 | 09:30 |
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | FLU:11_18, FLU:5_11   | Tomorrow | Tomorrow |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM     | 09:00 | 09:30 |
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | FLU:18:40, FLU:5_11   | Tomorrow | Tomorrow |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM     | 09:00 | 09:30 |
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | FLU:64_80, RSV:Adult   | Tomorrow | Tomorrow |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM     | 09:00 | 09:30 |
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | COVID, RSV:Adult       | Tomorrow | Tomorrow |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM     | 09:00 | 09:30 |
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | FLU:11_18, RSV:Adult   | Tomorrow | Tomorrow |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM     | 09:00 | 09:30 |
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | FLU:18:40, RSV:Adult   | Tomorrow | Tomorrow |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM     | 09:00 | 09:30 |
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | FLU:64_80, COVID   | Tomorrow | Tomorrow |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM     | 09:00 | 09:30 |
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | COVID, COVID       | Tomorrow | Tomorrow |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM     | 09:00 | 09:30 |
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | FLU:11_18, COVID   | Tomorrow | Tomorrow |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM     | 09:00 | 09:30 |
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | FLU:18:40, COVID   | Tomorrow | Tomorrow |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM     | 09:00 | 09:30 |
    # -- END OPTIMAL
    # SUBOPTIMAL
    # Other unavailable combinations involving 'FLU:2_3', that COULD be available in a best fit algorithm...
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | FLU:2_3, COVID        | Tomorrow | Tomorrow |
    Then the response should be empty
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | FLU:2_3, RSV:Adult    | Tomorrow | Tomorrow |
    Then the response should be empty
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | FLU:2_3, FLU:64_80    | Tomorrow | Tomorrow |
    Then the response should be empty
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | FLU:2_3, FLU:11_18    | Tomorrow | Tomorrow |
    Then the response should be empty
    When I query availability by days
      | Site                                 | Attendee Services     | From     | Until    |
      | 6188c242-acfa-4dc2-860a-ead658bfe187 | FLU:2_3, FLU:18:40    | Tomorrow | Tomorrow |
    Then the response should be empty
    # -- END SUBOPTIMAL
    

  Scenario: One booked slot breaks a consecutive pair
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'b4f1093b-83fc-4f99-9bd2-7c29080254db'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:30 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made for site 'b4f1093b-83fc-4f99-9bd2-7c29080254db'
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 09:10 | 10       | RSV:Adult | 56345-11111 |
    When I query availability by days
      | Site                                 | Attendee Services   | From     | Until             |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | RSV:Adult,RSV:Adult | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date              | Blocks | From  | Until |

  Scenario: Booking at the start still allows later consecutive slots
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'b4f1093b-83fc-4f99-9bd2-7c29080254db'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:30 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made for site 'b4f1093b-83fc-4f99-9bd2-7c29080254db'
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 09:00 | 10       | RSV:Adult | 56345-11111 |
    When I query availability by days
      | Site                                 | Attendee Services   | From     | Until             |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | RSV:Adult,RSV:Adult | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM     | 09:10 | 12:00 |

  Scenario: Booking at the end still allows earlier consecutive slots
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'b4f1093b-83fc-4f99-9bd2-7c29080254db'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:30 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made for site 'b4f1093b-83fc-4f99-9bd2-7c29080254db'
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 09:20 | 10       | RSV:Adult | 56345-11111 |
    When I query availability by days
      | Site                                 | Attendee Services   | From     | Until             |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | RSV:Adult,RSV:Adult | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM     | 09:00 | 12:00 |

  Scenario: Two non-adjacent bookings split availability into two valid windows
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'b4f1093b-83fc-4f99-9bd2-7c29080254db'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:50 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made for site 'b4f1093b-83fc-4f99-9bd2-7c29080254db'
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 09:10 | 10       | RSV:Adult | 56345-11111 |
      | Tomorrow    | 09:30 | 10       | RSV:Adult | 12345-11111 |
    When I query availability by days
      | Site                                 | Attendee Services   | From     | Until             |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | RSV:Adult,RSV:Adult | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |

  Scenario: One booking leaves valid consecutive pairs
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'b4f1093b-83fc-4f99-9bd2-7c29080254db'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 11:40 | 12:30 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made for site 'b4f1093b-83fc-4f99-9bd2-7c29080254db'
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 12:00 | 10       | RSV:Adult | 56345-11111 |
    When I query availability by days
      | Site                                 | Attendee Services   | From     | Until             |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | RSV:Adult,RSV:Adult | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | AM,PM  | 11:40 | 12:30 |

  Scenario: Only two slots exist and one is booked
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'b4f1093b-83fc-4f99-9bd2-7c29080254db'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:20 | RSV:Adult  | 10          | 1        |
    And the following bookings have been made for site 'b4f1093b-83fc-4f99-9bd2-7c29080254db'
      | Date        | Time  | Duration | Service   | Reference   |
      | Tomorrow    | 09:00 | 10       | RSV:Adult | 56345-11111 |
    When I query availability by days
      | Site                                 | Attendee Services   | From     | Until             |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | RSV:Adult,RSV:Adult | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |

  Scenario: One session too short, another session valid
    Given The following sites exist in the system
      | Site                                 | Name   | Address      | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude | Latitude | Type        |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | Site-A | 1A Site Lane | 0113 1111111 | 15N     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | -60       | -60      | GP Practice |
    And the following sessions exist for site 'b4f1093b-83fc-4f99-9bd2-7c29080254db'
      | Date              | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow          | 09:00 | 09:10 | RSV:Adult  | 10          | 1        |
      | Tomorrow          | 13:00 | 13:20 | RSV:Adult  | 10          | 1        |
    When I query availability by days
      | Site                                 | Attendee Services   | From     | Until             |
      | b4f1093b-83fc-4f99-9bd2-7c29080254db | RSV:Adult,RSV:Adult | Tomorrow | 2 days from today |
    Then the following single site availability by days is returned
      | Date     | Blocks | From  | Until |
      | Tomorrow | PM     | 12:00 | 13:20 |
