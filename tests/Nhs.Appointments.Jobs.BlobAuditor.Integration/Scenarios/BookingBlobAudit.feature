Feature: Blob Audit Booking Data

  Scenario: Files appear in Blob
    When I add a file to 'booking_data' container
    Then a lease token is present in 'blob_audit' container with application 'booking_auditor'  
    And files appear in blob 
    
  Scenario: Files appear in Blob
    When I add multiple files to 'booking_data' container
    Then a lease token is present in 'blob_audit' container with application 'booking_auditor'  
    And files appear in blob 
    And field 'contactDetails[]/value' is not present
    
  Scenario: Files appear in Blob
    When I add a file to 'booking_data' container
    And I update that file
    And I update that file 
    Then a lease token is present in 'blob_audit' container with application 'booking_auditor'  
    And files appear in blob 
    And field 'contactDetails[]/value' is not present
  
    
