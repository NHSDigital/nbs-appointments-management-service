Feature: Site search

  Scenario: Retrieve all sites within a designated distance
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | Attributes                   | Longitude   | Latitude  |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 5914b64a-66bb-4ee2-ab8a-94958c1fdfcb | Site-3 | 3 Roadside | 0113 3333333 | L12     | R3     | ICB3 | accessibility/attr_one=false | -0.13086317 | 51.583479 |
      | 10a54cc1-f052-4c7b-bfc8-de4e5ee7e193 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    When I make the following request without access needs
      | Max Records | Search Radius | Longitude | Latitude |
      | 50          | 6000          | 0.082     | 51.5     |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | Attributes                   | Longitude   | Latitude  | Distance |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-1 | 1 Roadside | 0113 1111111 | J12     | R1     | ICB1 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-2 | 2 Roadside | 0113 2222222 | K12     | R2     | ICB2 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | 10a54cc1-f052-4c7b-bfc8-de4e5ee7e193 | Site-4 | 4 Roadside | 0113 4444444 | M12     | R4     | ICB4 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Only return the number of sites requested
    Given The following sites exist in the system
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | Attributes                   | Longitude    | Latitude   |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-5 | 5 Roadside | 0113 5555555 | J12     | R5     | ICB5 | accessibility/attr_one=true  | -0.082750916 | -51.494056 |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-6 | 6 Roadside | 0113 6666666 | K12     | R6     | ICB6 | accessibility/attr_one=false | -0.14566747  | -51.482472 |
      | 10a54cc1-f052-4c7b-bfc8-de4e5ee7e193 | Site-7 | 7 Roadside | 0113 7777777 | L12     | R7     | ICB7 | accessibility/attr_one=true  | 0.13086317   | -51.583479 |
      | 5914b64a-66bb-4ee2-ab8a-94958c1fdfcb | Site-8 | 8 Roadside | 0113 8888888 | M12     | R8     | ICB8 | accessibility/attr_one=false | -0.040992272 | -51.455788 |
    When I make the following request without access needs
      | Max Records | Search Radius | Longitude | Latitude |
      | 2           | 100000        | -0.082    | -51.5    |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | Attributes                   | Longitude    | Latitude   | Distance |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-5 | 5 Roadside | 0113 5555555 | J12     | R5     | ICB5 | accessibility/attr_one=true  | -0.082750916 | -51.494056 | 662      |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-6 | 6 Roadside | 0113 6666666 | K12     | R6     | ICB6 | accessibility/attr_one=false | -0.14566747  | -51.482472 | 4819     |

  Scenario: Only return sites with requested access needs
    Given The following sites exist in the system
      | Site                                 | Name    | Address     | PhoneNumber  | OdsCode | Region | ICB   | Attributes                                               | Longitude | Latitude |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-9  | 9 Roadside  | 0113 9999999 | J12     | R9     | ICB9  | accessibility/attr_one=true,accessibility/attr_two=true  | -0.0827   | -51.5    |
      | 6877d86e-c2df-4def-8508-e1eccf0ea6be | Site-10 | 10 Roadside | 0113 1010101 | K12     | R10    | ICB10 | accessibility/attr_one=true,accessibility/attr_two=false | -0.0827   | -51.5    |
    When I make the following request with access needs
      | Max Records | Search Radius | Longitude | Latitude | AccessNeeds       |
      | 50          | 100000        | -0.082    | -51.5    | attr_one,attr_two |
    Then the following sites and distances are returned
      | Site                                 | Name   | Address    | PhoneNumber  | OdsCode | Region | ICB  | Attributes                                              | Longitude | Latitude | Distance |
      | beeae4e0-dd4a-4e3a-8f4d-738f9418fb51 | Site-9 | 9 Roadside | 0113 9999999 | J12     | R9     | ICB9 | accessibility/attr_one=true,accessibility/attr_two=true | -0.0827   | -51.5    | 48       |
