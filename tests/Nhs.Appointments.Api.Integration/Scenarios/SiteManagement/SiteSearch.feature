Feature: Site search
  
  Scenario: Retrieve all sites within a designated distance
    Given The following sites
      | Site | Name   | Address    | Longitude   | Latitude  |
      | A    | Site-A | 1 Roadside | 0.082750916 | 51.494056 |
      | B    | Site-B | 2 Roadside | 0.14566747  | 51.482472 |
      | C    | Site-C | 3 Roadside | -0.13086317 | 51.583479 |
      | D    | Site-D | 4 Roadside | 0.040992272 | 51.455788 |
    When I make the following request
      | Max Records | Search Radius | Longitude | Latitude |
      | 50          | 6000          | 0.082     | 51.5     |
    Then the following sites and distances are returned
      | Site | Name   | Address    | Longitude   | Latitude  | Distance |
      | A    | Site-A | 1 Roadside | 0.082750916 | 51.494056 | 663      |
      | B    | Site-B | 2 Roadside | 0.14566747  | 51.482472 | 4833     |
      | D    | Site-D | 4 Roadside | 0.040992272 | 51.455788 | 5684     |
  
  Scenario: Only return the number of sites requested
    Given The following sites
      | Site | Name   | Address    | Longitude    | Latitude  |
      | W    | Site-W | 5 Roadside | -0.082750916 | -51.494056 |
      | X    | Site-X | 6 Roadside | -0.14566747  | -51.482472 |
      | Y    | Site-Y | 7 Roadside | 0.13086317   | -51.583479 |
      | Z    | Site-Z | 8 Roadside | -0.040992272 | -51.455788 |
    When I make the following request
      | Max Records | Search Radius | Longitude | Latitude |
      | 2           | 100000        | -0.082    | -51.5    |
    Then the following sites and distances are returned
      | Site | Name   | Address    | Longitude    | Latitude   | Distance |
      | W    | Site-W | 5 Roadside | -0.082750916 | -51.494056 | 663      |
      | X    | Site-X | 6 Roadside | -0.14566747  | -51.482472 | 4833     |