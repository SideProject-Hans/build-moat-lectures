Feature: SDD test stack smoke check
    Confirms the Reqnroll acceptance-test stack is wired and can execute scenarios.
    This is infrastructure only — real QR Code behaviour is added during feature implementation.

    Scenario: Acceptance project runs
        Given the acceptance test stack is configured
        Then the acceptance project runs
