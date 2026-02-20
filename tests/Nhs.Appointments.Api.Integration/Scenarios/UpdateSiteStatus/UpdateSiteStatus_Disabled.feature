Feature: Update Site Status

  Scenario: Update Status
    Given the default site exists
    When I update the site status for the default site to 'Offline'
    Then the call should fail with 501
