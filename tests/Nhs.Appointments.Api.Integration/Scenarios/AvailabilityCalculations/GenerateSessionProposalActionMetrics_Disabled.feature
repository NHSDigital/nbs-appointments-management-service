Feature: Availability Edit Proposal Disabled

  Scenario: Recalculate availability change proposal
    When I propose an availability edit
    Then the call should fail with 501
