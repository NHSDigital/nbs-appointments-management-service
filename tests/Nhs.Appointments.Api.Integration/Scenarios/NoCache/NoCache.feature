Feature: No Cache Headers

  Scenario: API response contains no cache headers
    Given the default site exists
    When I query for a site
    Then the call should be 200 with no cache headers
