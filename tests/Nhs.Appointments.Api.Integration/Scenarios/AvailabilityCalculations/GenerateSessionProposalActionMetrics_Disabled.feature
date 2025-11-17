Feature: Availability Edit Proposal Disabled

  Scenario: Recalculate availability change proposal
    When I request the edit proposal endpoint
    Then the call should fail with 501
