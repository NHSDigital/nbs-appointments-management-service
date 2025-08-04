Feature: Update Site Status

  Scenario: Update Status
    Given a new site is configured for MYA
    When I update the site status to 'Offline'
    Then the site should have an updated site status
