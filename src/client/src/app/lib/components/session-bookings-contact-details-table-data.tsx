'use client';
import { jsDateFormat, toTimeFormat } from '@services/timeService';
import { AttendeeDetails, Booking, ClinicalService, ContactItem } from '@types';
import Link from 'next/link';
import { Table } from 'nhsuk-react-components';
import { ReactNode } from 'react';

type Props = {
  bookings: Booking[];
  clinicalServices: ClinicalService[];
  displayAction: boolean;
  site: string;
};

export const SessionBookingsContactDetailsTableData = ({
  bookings,
  clinicalServices,
  displayAction,
  site,
}: Props) => {
  const mapNameAndNHSNumber = (attendeeDetails: AttendeeDetails): ReactNode => {
    return (
      <span>
        {attendeeDetails.firstName} {attendeeDetails.lastName}
        <br />
        {attendeeDetails.nhsNumber}
      </span>
    );
  };

  const mapContactDetails = (contactDetails: ContactItem[]): ReactNode => {
    return contactDetails.map((details, key) => {
      return (
        <span key={key}>
          {details.value}
          <br />
        </span>
      );
    });
  };

  return (
    <>
      {bookings && bookings.length > 0 && (
        <Table>
          <Table.Head>
            <Table.Row>
              <Table.Cell>Time</Table.Cell>
              <Table.Cell>Name and NHS number</Table.Cell>
              <Table.Cell>Date of birth</Table.Cell>
              <Table.Cell>Contact details</Table.Cell>
              <Table.Cell>Services</Table.Cell>
              {displayAction && <Table.Cell>Action</Table.Cell>}
            </Table.Row>
          </Table.Head>
          <Table.Body>
            {bookings.map(booking => (
              <Table.Row key={booking.reference}>
                <Table.Cell>{toTimeFormat(booking.from)}</Table.Cell>
                <Table.Cell>
                  {mapNameAndNHSNumber(booking.attendeeDetails)}
                </Table.Cell>
                <Table.Cell>
                  {jsDateFormat(booking.attendeeDetails.dateOfBirth)}
                </Table.Cell>
                <Table.Cell>
                  {booking.contactDetails && booking.contactDetails.length > 0
                    ? mapContactDetails(booking.contactDetails)
                    : 'Not provided'}
                </Table.Cell>
                <Table.Cell>
                  {clinicalServices.find(c => c.value === booking.service)
                    ?.label ?? booking.service}
                </Table.Cell>
                {displayAction && (
                  <Table.Cell>
                    <Link
                      key={`cancel-${booking.reference}`}
                      href={`/site/${site}/appointment/${booking.reference}/cancel`}
                    >
                      Cancel
                    </Link>
                  </Table.Cell>
                )}
              </Table.Row>
            ))}
          </Table.Body>
        </Table>
      )}
    </>
  );
};
