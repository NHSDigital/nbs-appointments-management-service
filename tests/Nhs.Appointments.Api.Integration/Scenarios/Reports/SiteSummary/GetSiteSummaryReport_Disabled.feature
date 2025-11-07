Feature: Download Site Summary Report

  Scenario: Cannot download reports when toggle is disabled
    Given the site is configured for MYA
    When I request a site summary report for the following dates
      | Start Date | End Date          |
      | Tomorrow   | 2 days from today |
    Then the call should fail with 501
