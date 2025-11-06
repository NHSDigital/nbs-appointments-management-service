Feature: Download Site Summary Report

  Scenario: Can download reports when toggle is enabled
    Given the site is configured for MYA
    When I request a site summary report for the following dates
      | Start Date | End Date          |
      | Tomorrow   | 2 days from today |
    Then the call should be successful
    And the report has the following headers
      | Site Name | Site Type | ICB | ICB Name | Region | Region Name | ODS Code | Longitude | Latitude |
    And the report has the following headers
      | RSV:Adult Booked | COVID:5_11 Booked | COVID:12_17 Booked | COVID:18+ Booked | FLU:18_64 Booked | FLU:65+ Booked | COVID_FLU:18_64 Booked | COVID_FLU:65+ Booked | FLU:2_3 Booked |
    And the report has the following headers
      | Total Bookings | Cancelled | Maximum Capacity |
    And the report has the following headers
      | RSV:Adult Capacity | COVID:5_11 Capacity | COVID:12_17 Capacity | COVID:18+ Capacity | FLU:18_64 Capacity | FLU:65+ Capacity | COVID_FLU:18_64 Capacity | COVID_FLU:65+ Capacity | FLU:2_3 Capacity |
