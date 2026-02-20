Feature: Download Site Summary Report

  Scenario: Can download reports when toggle is enabled
    Given the following default site exists in the system
      | Name                 | Type        | Region | ICB  |
      | Site Download Report | GP Pharmacy | R1     | ICB1 |
    And the following default site reports exist in the system
      | Date     |
      | Tomorrow |
    When I request a site summary report for the following dates
      | Start Date | End Date          |
      | Tomorrow   | 2 days from today |
    Then the call should be successful
    And the report has the following headers
      | Site Name | Site Type | ICB | ICB Name | Region | Region Name | ODS Code | Longitude | Latitude |
    And the report has the following headers
      | RSV:Adult Booked | RSV:Adult Capacity | Total Bookings | Cancelled | Maximum Capacity |
    And the report contains a row with the following data
      | Name                 | Site Type   | ICB  | ICB Name                | Region | Region Name | RSV:Adult Booked | RSV:Adult Capacity | Total Bookings | Cancelled | Max Capacity |
      | Site Download Report | GP Pharmacy | ICB1 | Integrated Care Board 1 | R1     | Region 1    | 60               | 40                 | 60             | 3         | 100          |
