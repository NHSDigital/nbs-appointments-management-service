Feature: Update Site Status

  Scenario: Update Status
    Given the default site exists
    When I update the site status for the default site to 'Offline'
    Then the default site should have an updated site status

  Scenario: Returns bad request with invalid site status
    Given the default site exists
    When I update the site status for the default site to 'InvalidStatus'
    Then the call should fail with 400
