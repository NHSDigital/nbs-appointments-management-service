Feature: Update Site Status

  Scenario: Update Status
    Given a new site is configured for MYA
    When I update the site status to 'Offline'
    Then the site should have an updated site status

  Scenario: Returns bad request with invalid site status
    Given a new site is configured for MYA
    When I update the site status to 'InvalidStatus'
    Then the call should fail with 400
