export type Booking = {
    reference: string,
    from: string,
    duration: number,
    service: string,
    site: string,
    sessionHolder: string,
    outcome: string,
    attendeeDetails: AttendeeDetails
}

export type AttendeeDetails = {
    nhsNumber: string,
    firstName: string,
    lastName: string,
    dateOfBirth: string
}