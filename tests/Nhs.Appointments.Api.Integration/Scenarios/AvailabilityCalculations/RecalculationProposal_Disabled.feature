Feature: Availability Recalculation Proposal Disabled

  Scenario: Recalculate availability change proposal
    When I request recalculation proposal endpoint
    Then the call should fail with 404
