Feature: Site search
  
  Scenario: Retrieve all sites within a designated distance
    Given The following sites exist in the system
      | Site | Name   | Address    | PhoneNumber  | Region | ICB  | Attributes                   | Longitude   | Latitude  |
      | 1    | Site-1 | 1 Roadside | 0113 1111111 | R1     | ICB1 | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 2    | Site-2 | 2 Roadside | 0113 2222222 | R2     | ICB2 | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 3    | Site-3 | 3 Roadside | 0113 3333333 | R3     | ICB3 | accessibility/attr_one=false | -0.13086317 | 51.583479 |
      | 4    | Site-4 | 4 Roadside | 0113 4444444 | R4     | ICB4 | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    When I make the following request without access needs
      | Max Records | Search Radius | Longitude | Latitude |
      | 50          | 6000          | 0.082     | 51.5     |
    Then the following sites and distances are returned
      | Site | Name   | Address    |  PhoneNumber  | Region | ICB  | Attributes                   | Longitude   | Latitude  | Distance |
      | 1    | Site-1 | 1 Roadside |  0113 1111111 | R1     | ICB1 | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 662      |
      | 2    | Site-2 | 2 Roadside |  0113 2222222 | R2     | ICB2 | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4819     |
      | 4    | Site-4 | 4 Roadside |  0113 4444444 | R4     | ICB4 | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5677     |

  Scenario: Only return the number of sites requested
    Given The following sites exist in the system
      | Site | Name   | Address    | PhoneNumber  | Region | ICB  | Attributes                   | Longitude    | Latitude   |
      | 5    | Site-5 | 5 Roadside | 0113 5555555 | R5     | ICB5 | accessibility/attr_one=true  | -0.082750916 | -51.494056 |
      | 6    | Site-6 | 6 Roadside | 0113 6666666 | R6     | ICB6 | accessibility/attr_one=false | -0.14566747  | -51.482472 |
      | 7    | Site-7 | 7 Roadside | 0113 7777777 | R7     | ICB7 | accessibility/attr_one=true  | 0.13086317   | -51.583479 |
      | 8    | Site-8 | 8 Roadside | 0113 8888888 | R8     | ICB8 | accessibility/attr_one=false | -0.040992272 | -51.455788 |
    When I make the following request without access needs
      | Max Records | Search Radius | Longitude | Latitude |
      | 2           | 100000        | -0.082    | -51.5    |
    Then the following sites and distances are returned
      | Site | Name   | Address    | PhoneNumber  | Region | ICB  | Attributes                   | Longitude    | Latitude   | Distance |
      | 5    | Site-5 | 5 Roadside | 0113 5555555 | R5     | ICB5 | accessibility/attr_one=true  | -0.082750916 | -51.494056 | 662      |
      | 6    | Site-6 | 6 Roadside | 0113 6666666 | R6     | ICB6 | accessibility/attr_one=false | -0.14566747  | -51.482472 | 4819     |

  Scenario: Only return sites with requested access needs
    Given The following sites exist in the system
      | Site  | Name    | Address     | PhoneNumber  | Region | ICB   | Attributes                                               | Longitude | Latitude |
      | 9     | Site-9  | 9 Roadside  | 0113 9999999 | R9     | ICB9  | accessibility/attr_one=true,accessibility/attr_two=true  | -0.0827   | -51.5    |
      | 10    | Site-10 | 10 Roadside | 0113 1010101 | R10    | ICB10 | accessibility/attr_one=true,accessibility/attr_two=false | -0.0827   | -51.5    |
    When I make the following request with access needs
      | Max Records | Search Radius | Longitude | Latitude | AccessNeeds       |
      | 50          | 100000        | -0.082   | -51.5    | attr_one,attr_two |
    Then the following sites and distances are returned
      | Site | Name   | Address    | PhoneNumber  | Region | ICB  | Attributes                                              | Longitude | Latitude | Distance |
      | 9    | Site-9 | 9 Roadside | 0113 9999999 | R9     | ICB9 | accessibility/attr_one=true,accessibility/attr_two=true | -0.0827   | -51.5    | 48       |
