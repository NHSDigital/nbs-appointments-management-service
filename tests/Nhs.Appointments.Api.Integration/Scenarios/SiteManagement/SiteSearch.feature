Feature: Site search

  Scenario: Retrieve all sites within a designated distance
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 5914b64a-66bb-4ee2-ab8a-94958c1fdfcb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | -0.13086317 | 51.583479 |
      | 10a54cc1-f052-4c7b-bfc8-de4e5ee7e193 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    When I make the following request without access needs
      | Max Records | Search Radius | Longitude | Latitude |
      | 50          | 6000          | 0.082     | 51.5     |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | 10a54cc1-f052-4c7b-bfc8-de4e5ee7e193 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Only return the number of sites requested
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude    | Latitude   |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-5 | 5 Roadside | 0113 5555555 | J12     | R5     | ICB5 | Info 5                 | accessibility/attr_one=true  | -0.082750916 | -51.494056 |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-6 | 6 Roadside | 0113 6666666 | K12     | R6     | ICB6 | Info 6                 | accessibility/attr_one=false | -0.14566747  | -51.482472 |
      | 10a54cc1-f052-4c7b-bfc8-de4e5ee7e193 | Site-7 | 7 Roadside | 0113 7777777 | L12     | R7     | ICB7 | Info 7                 | accessibility/attr_one=true  | 0.13086317   | -51.583479 |
      | 5914b64a-66bb-4ee2-ab8a-94958c1fdfcb | Site-8 | 8 Roadside | 0113 8888888 | M12     | R8     | ICB8 | Info 8                 | accessibility/attr_one=false | -0.040992272 | -51.455788 |
    When I make the following request without access needs
      | Max Records | Search Radius | Longitude | Latitude |
      | 2           | 100000        | -0.082    | -51.5    |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude    | Latitude   | Distance |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-5 | 5 Roadside | 0113 5555555 | J12     | R5     | ICB5 | Info 5                 | accessibility/attr_one=true  | -0.082750916 | -51.494056 | 662      |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-6 | 6 Roadside | 0113 6666666 | K12     | R6     | ICB6 | Info 6                 | accessibility/attr_one=false | -0.14566747  | -51.482472 | 4819     |

  Scenario: Only return sites with requested access needs
    Given The following sites exist in the system
      | Site                                 | Name    | Address     | PhoneNumber  | OdsCode | Region | ICB   | InformationForCitizens | Accessibilities                                          | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-9  | 9 Roadside  | 0113 9999999 | J12     | R9     | ICB9  | Info 9                 | accessibility/attr_one=true,accessibility/attr_two=true  | -0.0827   | -51.5    |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-10 | 10 Roadside | 0113 1010101 | K12     | R10    | ICB10 | Info 10                | accessibility/attr_one=true,accessibility/attr_two=false | -0.0827   | -51.5    |
    When I make the following request with access needs
      | Max Records | Search Radius | Longitude | Latitude | AccessNeeds       |
      | 50          | 100000        | -0.082    | -51.5    | attr_one,attr_two |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                         | Longitude | Latitude | Distance |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-9 | 9 Roadside | 0113 9999999 | J12     | R9     | ICB9 | Info 9                 | accessibility/attr_one=true,accessibility/attr_two=true | -0.0827   | -51.5    | 48       |

  Scenario: Retrieve sites by service filter - single result
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | a03982ab-f9a8-4d4b-97ca-419d1154896f | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
    And the following sessions exist for site 'a03982ab-f9a8-4d4b-97ca-419d1154896f'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    When I make the following request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    |
      | 3           | 6000          | 0.082     | 51.5     | COVID   | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | a03982ab-f9a8-4d4b-97ca-419d1154896f | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |

  Scenario: Retrieve sites by service filter - multiple results limited to max records ordered by distance
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 3ac1981b-5d62-424a-b403-9d08a40739ce | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 3525af0d-9d89-4b32-ad6b-b85ae94589dc | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | f178d668-a8d7-4fa6-a4b1-b886feef29a6 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
      | 156141af-89ab-4a30-83d4-a4d27a8322c2 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
    And the following sessions exist for site '156141af-89ab-4a30-83d4-a4d27a8322c2'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site 'f178d668-a8d7-4fa6-a4b1-b886feef29a6'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site '3525af0d-9d89-4b32-ad6b-b85ae94589dc'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site '3ac1981b-5d62-424a-b403-9d08a40739ce'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    When I make the following request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    |
      | 3           | 6000          | 0.082     | 51.5     | COVID   | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 156141af-89ab-4a30-83d4-a4d27a8322c2 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | 3525af0d-9d89-4b32-ad6b-b85ae94589dc | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 | 3849     |
      | 3ac1981b-5d62-424a-b403-9d08a40739ce | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
    When I make the following request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    |
      | 4           | 6000          | 0.082     | 51.5     | COVID   | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 156141af-89ab-4a30-83d4-a4d27a8322c2 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | 3525af0d-9d89-4b32-ad6b-b85ae94589dc | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 | 3849     |
      | 3ac1981b-5d62-424a-b403-9d08a40739ce | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | f178d668-a8d7-4fa6-a4b1-b886feef29a6 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Retrieve sites by service filter - limits results to only those that support the service
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | b0fa3eaa-cbab-4736-90dd-31922a021074 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 319eb942-1bcd-4d9b-b8b2-777f06d63320 | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | 8f3259bf-e44e-43e6-9837-54a5c87198c7 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
      | 55ad05a8-4fb5-47e1-a961-d18f4008862b | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
    And the following sessions exist for site 'b0fa3eaa-cbab-4736-90dd-31922a021074'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2-3    | 5           | 1        |
    And the following sessions exist for site '319eb942-1bcd-4d9b-b8b2-777f06d63320'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
    And the following sessions exist for site '8f3259bf-e44e-43e6-9837-54a5c87198c7'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site '55ad05a8-4fb5-47e1-a961-d18f4008862b'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    When I make the following request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    |
      | 4           | 6000          | 0.082     | 51.5     | COVID   | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 55ad05a8-4fb5-47e1-a961-d18f4008862b | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | 8f3259bf-e44e-43e6-9837-54a5c87198c7 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Retrieve sites by service filter - finds a site that isn't the closest because it supports the service
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 12e87824-a2ff-4257-92f3-ee1667c271a3 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 4aeedaf7-48a8-4071-955c-93ccbcbc925c | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | 1950e7f1-356c-4017-ba62-62f3f973681f | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
      | 78d28642-f429-4164-b758-f770b3dcd705 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
    And the following sessions exist for site '12e87824-a2ff-4257-92f3-ee1667c271a3'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2-3    | 5           | 1        |
    And the following sessions exist for site '4aeedaf7-48a8-4071-955c-93ccbcbc925c'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
    And the following sessions exist for site '1950e7f1-356c-4017-ba62-62f3f973681f'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site '78d28642-f429-4164-b758-f770b3dcd705'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:8-16   | 5           | 1        |
#    Prove old endpoint doesn't return a supported service site
    When I make the following request without access needs
      | Max Records | Search Radius | Longitude | Latitude |
      | 1           | 6000          | 0.082     | 51.5     |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 78d28642-f429-4164-b758-f770b3dcd705 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
#    Prove new endpoint returns a supported service site
    When I make the following request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    |
      | 1           | 6000          | 0.082     | 51.5     | COVID   | Tomorrow | Tomorrow |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 1950e7f1-356c-4017-ba62-62f3f973681f | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Retrieve sites by service filter - no results if the only site that supports the service is outside the search radius
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 12e87824-a2ff-4257-92f3-ee1667c271a3 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 4aeedaf7-48a8-4071-955c-93ccbcbc925c | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | 1950e7f1-356c-4017-ba62-62f3f973681f | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
      | 78d28642-f429-4164-b758-f770b3dcd705 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
    And the following sessions exist for site '12e87824-a2ff-4257-92f3-ee1667c271a3'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2-3    | 5           | 1        |
    And the following sessions exist for site '4aeedaf7-48a8-4071-955c-93ccbcbc925c'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
    And the following sessions exist for site '1950e7f1-356c-4017-ba62-62f3f973681f'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site '78d28642-f429-4164-b758-f770b3dcd705'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:8-16   | 5           | 1        |
    When I make the following request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    |
      | 1           | 5000          | 0.082     | 51.5     | COVID   | Tomorrow | Tomorrow |
    Then no sites are returned

  Scenario: Retrieve sites by service filter - no results if no sessions
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | beeae4e0-dd4a-4e4a-8f4d-738f9418fb51 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 6877d86e-c2df-1def-8508-e1eccf0ea6be | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 5914b64a-66bb-4ee2-ab8a-94958c1fdfcb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | -0.13086317 | 51.583479 |
      | 10a54cc1-c052-4c7b-bfc8-de4e5ee7e193 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    When I make the following request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    |
      | 3           | 6000          | 0.082     | 51.5     | COVID   | Tomorrow | Tomorrow |
    Then no sites are returned

  Scenario: Retrieve sites by service filter - no results if no sessions for service exist
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 355ca42f-586c-4f7a-a274-4d53844e3e0c | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 8da01caa-f589-4914-9c4c-42d7adb185ae | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 96f49dd3-f0cb-4b1b-826d-d07065e14c86 | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | -0.13086317 | 51.583479 |
      | 7627459e-15b5-44e7-9318-1b1f3ca5c414 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for site '355ca42f-586c-4f7a-a274-4d53844e3e0c'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
    And the following sessions exist for site '8da01caa-f589-4914-9c4c-42d7adb185ae'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID-19   | 5           | 1        |
    And the following sessions exist for site '96f49dd3-f0cb-4b1b-826d-d07065e14c86'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2-3    | 5           | 1        |
    And the following sessions exist for site '7627459e-15b5-44e7-9318-1b1f3ca5c414'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COV        | 5           | 1        |
    When I make the following request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    |
      | 3           | 6000          | 0.082     | 51.5     | COVID   | Tomorrow | Tomorrow |
    Then no sites are returned

  Scenario: Retrieve sites by service filter - no results if no sessions for service exist in the date range required
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | b37000df-f261-40ce-b86b-68b31540e804 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 6092852c-b454-4249-9e12-0c0152056708 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 96e527f3-5791-4567-a6a7-6691f5dcecb5 | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | -0.13086317 | 51.583479 |
      | 38d0905e-9395-4954-ba72-0ae5a81ff876 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for site 'b37000df-f261-40ce-b86b-68b31540e804'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site '6092852c-b454-4249-9e12-0c0152056708'
      | Date                 | From  | Until | Services   | Slot Length | Capacity |
      | 4 days from today    | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site '96e527f3-5791-4567-a6a7-6691f5dcecb5'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site '38d0905e-9395-4954-ba72-0ae5a81ff876'
      | Date                 | From  | Until | Services     | Slot Length | Capacity |
      | 4 days from today    | 09:00 | 17:00 | COVID        | 5           | 1        |
    When I make the following request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From              | Until             |
      | 3           | 6000          | 0.082     | 51.5     | COVID   | 2 days from today | 3 days from today |
    Then no sites are returned
    
  Scenario: Retrieve sites by service filter - results when service sessions are on different days
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 9ac67f31-cc79-46c0-b0d2-e3be1d7b8caa | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | a1a30efa-9506-47aa-b6a6-fa0e74c848f4 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 45a73a10-87c1-4826-a740-e1ebc4585618 | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | -0.13086317 | 51.583479 |
      | 61da1c54-0b24-470a-8978-bb7c66a15816 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for site '9ac67f31-cc79-46c0-b0d2-e3be1d7b8caa'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site 'a1a30efa-9506-47aa-b6a6-fa0e74c848f4'
      | Date                 | From  | Until | Services   | Slot Length | Capacity |
      | 2 days from today    | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site '45a73a10-87c1-4826-a740-e1ebc4585618'
      | Date               | From  | Until | Services   | Slot Length | Capacity |
      | 3 days from today  | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site '61da1c54-0b24-470a-8978-bb7c66a15816'
      | Date                 | From  | Until | Services     | Slot Length | Capacity |
      | 4 days from today    | 09:00 | 17:00 | COVID        | 5           | 1        |
    When I make the following request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From        | Until             |
      | 3           | 6000          | 0.082     | 51.5     | COVID   | Tomorrow    | 4 days from today |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 9ac67f31-cc79-46c0-b0d2-e3be1d7b8caa | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | a1a30efa-9506-47aa-b6a6-fa0e74c848f4 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | 61da1c54-0b24-470a-8978-bb7c66a15816 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Retrieve sites by service filter - results when service sessions have multiple services
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 4dbaffc9-f476-494b-817a-37dc8aa151c9 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | b06f4be6-16e3-40be-b373-b67a47301185 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 2ba498ab-42b5-4536-9f11-796077922ce1 | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | -0.13086317 | 51.583479 |
      | d2f9f101-b145-42ef-93e0-ae449efb9a78 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for site '4dbaffc9-f476-494b-817a-37dc8aa151c9'
      | Date        | From  | Until | Services          | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult, COVID  | 5           | 1        |
    And the following sessions exist for site 'b06f4be6-16e3-40be-b373-b67a47301185'
      | Date                 | From  | Until | Services        | Slot Length | Capacity |
      | 2 days from today    | 09:00 | 17:00 | COVID, FLU:2-3  | 5           | 1        |
    And the following sessions exist for site '2ba498ab-42b5-4536-9f11-796077922ce1'
      | Date               | From  | Until | Services           | Slot Length | Capacity |
      | 3 days from today  | 09:00 | 17:00 | COVID, COVID:5-18  | 5           | 1        |
    And the following sessions exist for site 'd2f9f101-b145-42ef-93e0-ae449efb9a78'
      | Date                 | From  | Until | Services        | Slot Length | Capacity |
      | 4 days from today    | 09:00 | 17:00 | COVID, FLU:2-3  | 5           | 1        |
    When I make the following request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From        | Until             |
      | 3           | 6000          | 0.082     | 51.5     | COVID   | Tomorrow    | 4 days from today |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 4dbaffc9-f476-494b-817a-37dc8aa151c9 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | b06f4be6-16e3-40be-b373-b67a47301185 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | d2f9f101-b145-42ef-93e0-ae449efb9a78 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |
    
# This shows the temporary quicker solution for this hotfix 2.2.2 (APPT-1203) and how it isn't a perfect solution and should be fixed properly in the future
  Scenario: Retrieve sites by service filter - returns sites that are fully booked and support the service (intended behaviour)
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 2d1780ea-73cf-43c1-ad19-1f0cb288e35b | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 586bc02d-310a-4b02-a117-d0d104de16bb | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | a01e7aec-4721-410b-853d-1bed6ade4c3c | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | a5f2f93e-26e8-45ac-a09b-2485517f1d9c | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for site '2d1780ea-73cf-43c1-ad19-1f0cb288e35b'
      | Date        | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 09:10 | COVID     | 10           | 1       |
    And the following bookings have been made for site '2d1780ea-73cf-43c1-ad19-1f0cb288e35b'
      | Date        | Time  | Duration | Service | Reference   |
      | Tomorrow    | 09:00 | 10       | COVID   | 56345-11111 |
    And the following sessions exist for site '586bc02d-310a-4b02-a117-d0d104de16bb'
      | Date                 | From  | Until | Services  | Slot Length | Capacity |
      | 2 days from today    | 09:00 | 09:10 | COVID     | 10           | 1       |
    And the following bookings have been made for site '586bc02d-310a-4b02-a117-d0d104de16bb'
      | Date                 | Time  | Duration | Service | Reference   |
      | 2 days from today    | 09:00 | 10       | COVID   | 56345-22222 |
    And the following sessions exist for site 'a01e7aec-4721-410b-853d-1bed6ade4c3c'
      | Date               | From  | Until | Services  | Slot Length | Capacity |
      | 3 days from today  | 09:00 | 09:10 | COVID     | 10           | 1       |
    And the following bookings have been made for site 'a01e7aec-4721-410b-853d-1bed6ade4c3c'
      | Date                 | Time  | Duration | Service | Reference   |
      | 3 days from today    | 09:00 | 10       | COVID   | 56345-33333 |
    And the following sessions exist for site 'a5f2f93e-26e8-45ac-a09b-2485517f1d9c'
      | Date                 | From  | Until | Services | Slot Length | Capacity |
      | 4 days from today    | 09:00 | 09:10 | COVID    | 10           | 1       |
    And the following bookings have been made for site 'a5f2f93e-26e8-45ac-a09b-2485517f1d9c'
      | Date                 | Time  | Duration | Service | Reference   |
      | 4 days from today    | 09:00 | 10       | COVID   | 56345-44444 |
    When I check daily availability for site '2d1780ea-73cf-43c1-ad19-1f0cb288e35b' for 'COVID' between 'Tomorrow' and '4 days from today'
    Then the following availability is returned for 'Tomorrow'
      | From  | Until | Count |
      | 00:00 | 12:00 | 0     |
      | 12:00 | 00:00 | 0     |
    When I check daily availability for site '586bc02d-310a-4b02-a117-d0d104de16bb' for 'COVID' between 'Tomorrow' and '4 days from today'
    Then the following availability is returned for '2 days from today'
      | From  | Until | Count |
      | 00:00 | 12:00 | 0     |
      | 12:00 | 00:00 | 0     |
    When I check daily availability for site 'a01e7aec-4721-410b-853d-1bed6ade4c3c' for 'COVID' between 'Tomorrow' and '4 days from today'
    Then the following availability is returned for '3 days from today'
      | From  | Until | Count |
      | 00:00 | 12:00 | 0     |
      | 12:00 | 00:00 | 0     |
    When I check daily availability for site 'a5f2f93e-26e8-45ac-a09b-2485517f1d9c' for 'COVID' between 'Tomorrow' and '4 days from today'
    Then the following availability is returned for '4 days from today'
      | From  | Until | Count |
      | 00:00 | 12:00 | 0     |
      | 12:00 | 00:00 | 0     |
    When I make the following request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From        | Until             |
      | 4           | 6000          | 0.082     | 51.5     | COVID   | Tomorrow    | 4 days from today |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 2d1780ea-73cf-43c1-ad19-1f0cb288e35b | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | a01e7aec-4721-410b-853d-1bed6ade4c3c | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 | 3849     |
      | 586bc02d-310a-4b02-a117-d0d104de16bb | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | a5f2f93e-26e8-45ac-a09b-2485517f1d9c | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Retrieve sites by service filter - returns sites that are partially booked and support the service (intended behaviour)
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 20e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 9bf7f58b-ca1a-425a-869e-7a574e183a2c | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 6beadf23-2c8c-4080-8be6-896c73634efb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | aa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following sessions exist for site '20e7b709-83c6-416b-b5d8-27d03222e1bf'
      | Date        | From  | Until | Services  | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 09:20 | COVID     | 10           | 1       |
    And the following bookings have been made for site '20e7b709-83c6-416b-b5d8-27d03222e1bf'
      | Date        | Time  | Duration | Service | Reference   |
      | Tomorrow    | 09:00 | 10       | COVID   | 56345-11111 |
    And the following sessions exist for site '9bf7f58b-ca1a-425a-869e-7a574e183a2c'
      | Date                 | From  | Until | Services  | Slot Length | Capacity |
      | 2 days from today    | 09:00 | 09:20 | COVID     | 10           | 1       |
    And the following bookings have been made for site '9bf7f58b-ca1a-425a-869e-7a574e183a2c'
      | Date                 | Time  | Duration | Service | Reference   |
      | 2 days from today    | 09:00 | 10       | COVID   | 56345-22222 |
    And the following sessions exist for site '6beadf23-2c8c-4080-8be6-896c73634efb'
      | Date               | From  | Until | Services  | Slot Length | Capacity |
      | 3 days from today  | 09:00 | 09:20 | COVID     | 10           | 1       |
    And the following bookings have been made for site '6beadf23-2c8c-4080-8be6-896c73634efb'
      | Date                 | Time  | Duration | Service | Reference   |
      | 3 days from today    | 09:00 | 10       | COVID   | 56345-33333 |
    And the following sessions exist for site 'aa8ceff5-d152-4687-b8ea-030df7d5efb1'
      | Date                 | From  | Until | Services | Slot Length | Capacity |
      | 4 days from today    | 09:00 | 09:20 | COVID    | 10           | 1       |
    And the following bookings have been made for site 'aa8ceff5-d152-4687-b8ea-030df7d5efb1'
      | Date                 | Time  | Duration | Service | Reference   |
      | 4 days from today    | 09:00 | 10       | COVID   | 56345-44444 |
    When I check daily availability for site '20e7b709-83c6-416b-b5d8-27d03222e1bf' for 'COVID' between 'Tomorrow' and '4 days from today'
    Then the following availability is returned for 'Tomorrow'
      | From  | Until | Count |
      | 00:00 | 12:00 | 1     |
      | 12:00 | 00:00 | 0     |
    When I check daily availability for site '9bf7f58b-ca1a-425a-869e-7a574e183a2c' for 'COVID' between 'Tomorrow' and '4 days from today'
    Then the following availability is returned for '2 days from today'
      | From  | Until | Count |
      | 00:00 | 12:00 | 1     |
      | 12:00 | 00:00 | 0     |
    When I check daily availability for site '6beadf23-2c8c-4080-8be6-896c73634efb' for 'COVID' between 'Tomorrow' and '4 days from today'
    Then the following availability is returned for '3 days from today'
      | From  | Until | Count |
      | 00:00 | 12:00 | 1     |
      | 12:00 | 00:00 | 0     |
    When I check daily availability for site 'aa8ceff5-d152-4687-b8ea-030df7d5efb1' for 'COVID' between 'Tomorrow' and '4 days from today'
    Then the following availability is returned for '4 days from today'
      | From  | Until | Count |
      | 00:00 | 12:00 | 1     |
      | 12:00 | 00:00 | 0     |
    When I make the following request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From        | Until             |
      | 4           | 6000          | 0.082     | 51.5     | COVID   | Tomorrow    | 4 days from today |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | 20e7b709-83c6-416b-b5d8-27d03222e1bf | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | 6beadf23-2c8c-4080-8be6-896c73634efb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 | 3849     |
      | 9bf7f58b-ca1a-425a-869e-7a574e183a2c | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | aa8ceff5-d152-4687-b8ea-030df7d5efb1 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Retrieve sites by service filter - returns no sites if they have orphaned bookings for that service, but no sessions for that service
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | dbd61ab0-281a-4df0-b1b7-6b6793f8e119 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | dfd0ac4e-d5ae-4c63-87c9-c84a84c9a7d1 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 5fdbbb82-45ee-4d0c-bbe0-57d25b55512a | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | 6f2108ef-d1b3-4479-a5dc-cf2bb68dceb9 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    And the following orphaned bookings exist for site 'dbd61ab0-281a-4df0-b1b7-6b6793f8e119'
      | Date        | Time  | Duration | Service | Reference   |
      | Tomorrow    | 09:00 | 10       | COVID   | 56345-11111 |
    And the following orphaned bookings exist for site 'dfd0ac4e-d5ae-4c63-87c9-c84a84c9a7d1'
      | Date                 | Time  | Duration | Service | Reference   |
      | 2 days from today    | 09:00 | 10       | COVID   | 56345-22222 |
    And the following orphaned bookings exist for site '5fdbbb82-45ee-4d0c-bbe0-57d25b55512a'
      | Date                 | Time  | Duration | Service | Reference   |
      | 3 days from today    | 09:00 | 10       | COVID   | 56345-33333 |
    And the following orphaned bookings exist for site '6f2108ef-d1b3-4479-a5dc-cf2bb68dceb9'
      | Date                 | Time  | Duration | Service | Reference   |
      | 4 days from today    | 09:00 | 10       | COVID   | 56345-44444 |
    When I make the following request with service filtering
      | Max Records | Search Radius | Longitude | Latitude | Service | From        | Until             |
      | 4           | 6000          | 0.082     | 51.5     | COVID   | Tomorrow    | 4 days from today |
    Then no sites are returned

  Scenario: Retrieve sites by service filter - only return sites with requested access needs
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  |
      | 9635bee5-895c-4368-a106-2d6bc1d74087 | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 7e7bc5ed-a188-499d-9eab-1566d9e3b972 | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=false | 0.13086317  | 51.483479 |
      | 3558dad9-d0a6-49f4-942a-c951d07eb283 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
      | ad8ef3bd-cf15-47a6-8510-8bffcd52bd7b | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
    And the following sessions exist for site '9635bee5-895c-4368-a106-2d6bc1d74087'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site '7e7bc5ed-a188-499d-9eab-1566d9e3b972'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site '3558dad9-d0a6-49f4-942a-c951d07eb283'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
    And the following sessions exist for site 'ad8ef3bd-cf15-47a6-8510-8bffcd52bd7b'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    When I make the following request with service filtering and with access needs
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    | AccessNeeds  |
      | 4           | 6000          | 0.082     | 51.5     | COVID   | Tomorrow | Tomorrow | attr_one     |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities              | Longitude   | Latitude  | Distance |
      | ad8ef3bd-cf15-47a6-8510-8bffcd52bd7b | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |

  Scenario: Retrieve sites by service filter - only return sites with all requested access needs
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                            | Longitude   | Latitude  |
      | 6997851c-0bcd-462e-b1f5-b1c02f24d37d | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | Info 2                 | accessibility/attr_one=false,accessibility/attr_two=true   | 0.14566747  | 51.482472 |
      | 1efc8a32-ad71-4066-821e-e535317c309a | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | Info 3                 | accessibility/attr_one=true,accessibility/attr_two=false   | 0.13086317  | 51.483479 |
      | ab746a05-345f-4c06-829b-2d5d52ec341b | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | Info 4                 | accessibility/attr_one=false,accessibility/attr_two=false  | 0.040992272 | 51.455788 |
      | 8eb79504-1545-4fd9-a358-430a649e0352 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true,accessibility/attr_two=true    | 0.082750916 | 51.494056 |
      | ae579504-1545-4fd9-a358-430a649e0354 | Site-5 | 5 Roadside | 0113 1111111 | N12     | R5     | ICB5 | Info 5                 | accessibility/attr_one=true,accessibility/attr_two=true    | 0.081750916 | 51.484056 |
    And the following sessions exist for site '6997851c-0bcd-462e-b1f5-b1c02f24d37d'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site '1efc8a32-ad71-4066-821e-e535317c309a'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site 'ab746a05-345f-4c06-829b-2d5d52ec341b'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | RSV:Adult  | 5           | 1        |
    And the following sessions exist for site '8eb79504-1545-4fd9-a358-430a649e0352'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | COVID      | 5           | 1        |
    And the following sessions exist for site 'ae579504-1545-4fd9-a358-430a649e0354'
      | Date        | From  | Until | Services   | Slot Length | Capacity |
      | Tomorrow    | 09:00 | 17:00 | FLU:2-3    | 5           | 1        |
    When I make the following request with service filtering and with access needs
      | Max Records | Search Radius | Longitude | Latitude | Service | From     | Until    | AccessNeeds        |
      | 4           | 6000          | 0.082     | 51.5     | COVID   | Tomorrow | Tomorrow | attr_one,attr_two  |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | InformationForCitizens | Accessibilities                                          | Longitude   | Latitude  | Distance |
      | 8eb79504-1545-4fd9-a358-430a649e0352 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | Info 1                 | accessibility/attr_one=true,accessibility/attr_two=true  | 0.082750916 | 51.494056 | 662      |
