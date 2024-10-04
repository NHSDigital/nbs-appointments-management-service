Feature: Site search
  
  Scenario: Retrieve all sites within a designated distance
    Given The following sites exist in the system
      | Site | Name   | Address    | Attributes                   | Longitude   | Latitude  |
      | 1    | Site-1 | 1 Roadside | accessibility/attr_one=true  | 0.082750916 | 51.494056 |
      | 2    | Site-2 | 2 Roadside | accessibility/attr_one=false | 0.14566747  | 51.482472 |
      | 3    | Site-3 | 3 Roadside | accessibility/attr_one=false | -0.13086317 | 51.583479 |
      | 4    | Site-4 | 4 Roadside | accessibility/attr_one=true  | 0.040992272 | 51.455788 |
    When I make the following request without access needs
      | Max Records | Search Radius | Longitude | Latitude |
      | 50          | 6000          | 0.082     | 51.5     |
    Then the following sites and distances are returned
      | Site | Name   | Address    | Attributes                   | Longitude   | Latitude  | Distance |
      | 1    | Site-1 | 1 Roadside | accessibility/attr_one=true  | 0.082750916 | 51.494056 | 663      |
      | 2    | Site-2 | 2 Roadside | accessibility/attr_one=false | 0.14566747  | 51.482472 | 4833     |
      | 4    | Site-4 | 4 Roadside | accessibility/attr_one=true  | 0.040992272 | 51.455788 | 5684     |

  Scenario: Only return the number of sites requested
    Given The following sites exist in the system
      | Site | Name   | Address    | Attributes                   | Longitude    | Latitude   |
      | 5    | Site-5 | 5 Roadside | accessibility/attr_one=true  | -0.082750916 | -51.494056 |
      | 6    | Site-6 | 6 Roadside | accessibility/attr_one=false | -0.14566747  | -51.482472 |
      | 7    | Site-7 | 7 Roadside | accessibility/attr_one=true  | 0.13086317   | -51.583479 |
      | 8    | Site-8 | 8 Roadside | accessibility/attr_one=false | -0.040992272 | -51.455788 |
    When I make the following request without access needs
      | Max Records | Search Radius | Longitude | Latitude |
      | 2           | 100000        | -0.082    | -51.5    |
    Then the following sites and distances are returned
      | Site | Name   | Address    | Attributes                   | Longitude    | Latitude   | Distance |
      | 5    | Site-5 | 5 Roadside | accessibility/attr_one=true  | -0.082750916 | -51.494056 | 663      |
      | 6    | Site-6 | 6 Roadside | accessibility/attr_one=false | -0.14566747  | -51.482472 | 4833     |

  Scenario: Only return sites with requested access needs
    Given The following sites exist in the system
      | Site  | Name    | Address     | Attributes                                               | Longitude | Latitude |
      | 9     | Site-9  | 9 Roadside  | accessibility/attr_one=true,accessibility/attr_two=true  | -0.0827   | -51.5    |
      | 10    | Site-10 | 10 Roadside | accessibility/attr_one=true,accessibility/attr_two=false | -0.0827   | -51.5    |
    When I make the following request with access needs
      | Max Records | Search Radius | Longitude | Latitude | AccessNeeds       |
      | 50          | 100000        | -0.082   | -51.5    | attr_one,attr_two |
    Then the following sites and distances are returned
      | Site | Name   | Address    | Attributes                                              | Longitude | Latitude | Distance |
      | 9    | Site-9 | 9 Roadside | accessibility/attr_one=true,accessibility/attr_two=true | -0.0827   | -51.5    | 49       |