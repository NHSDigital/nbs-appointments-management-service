import render from '@testing/render';
import { screen } from '@testing-library/react';
import {
  mockAvailabilityCreatedEvents,
  mockClinicalServices,
  mockMultipleServicesAvailabilityCreatedEvents,
  mockSite,
} from '@testing/data';
import {
  fetchAvailabilityCreatedEvents,
  fetchClinicalServices,
} from '@services/appointmentsService';
import { AvailabilityCreatedEvent, ClinicalService } from '@types';
import { AvailabilityCreatedEventsTable } from './availabilityCreatedEventsTable';

jest.mock('@services/appointmentsService');
const fetchAvailabilityCreatedEventsMock =
  fetchAvailabilityCreatedEvents as jest.Mock<
    Promise<AvailabilityCreatedEvent[]>
  >;

const fetchClinicalServicesMock = fetchClinicalServices as jest.Mock<
  Promise<ClinicalService[]>
>;

describe('Availability Created Events Table - multiple services', () => {
  beforeEach(() => {
    fetchAvailabilityCreatedEventsMock.mockResolvedValue(
      mockMultipleServicesAvailabilityCreatedEvents,
    );
    fetchClinicalServicesMock.mockReturnValue(
      Promise.resolve(mockClinicalServices),
    );
  });

  it('renders', async () => {
    const jsx = await AvailabilityCreatedEventsTable({
      siteId: mockSite.id,
    });

    render(jsx);

    expect(screen.getByRole('table')).toBeInTheDocument();
  });

  it('renders a table of availability periods', async () => {
    const jsx = await AvailabilityCreatedEventsTable({
      siteId: mockSite.id,
    });

    render(jsx);

    expect(fetchAvailabilityCreatedEventsMock).toHaveBeenCalledWith(
      mockSite.id,
    );
    expect(screen.getByRole('table')).toBeInTheDocument();

    const columns = ['Dates', 'Days', 'Services', 'Session type'];
    columns.forEach(column => {
      screen.getByRole('columnheader', { name: column });
    });

    expect(screen.getAllByRole('row')).toHaveLength(5);

    expect(
      screen.getByRole('row', {
        name: '1 Jan 2024 - 28 Feb 2024 Mon, Tue RSV Adult, Test Weekly repeating',
      }),
    );

    expect(
      screen.getByRole('row', {
        name: '1 Jan 2025 Wed RSV Adult, Test Single date',
      }),
    );

    expect(
      screen.getByRole('row', {
        name: '1 Mar 2024 - 30 Apr 2024 All RSV Adult Weekly repeating',
      }),
    );

    expect(
      screen.getByRole('row', {
        name: '16 Feb 2025 Sun RSV Adult Single date',
      }),
    );
  });
});

describe('Availability Created Events Table', () => {
  beforeEach(() => {
    fetchAvailabilityCreatedEventsMock.mockResolvedValue(
      mockAvailabilityCreatedEvents,
    );
    fetchClinicalServicesMock.mockReturnValue(
      Promise.resolve([{ label: 'RSV Adult', value: 'RSV:Adult' }]),
    );
  });

  it('renders', async () => {
    const jsx = await AvailabilityCreatedEventsTable({
      siteId: mockSite.id,
    });

    render(jsx);

    expect(screen.getByRole('table')).toBeInTheDocument();
  });

  it('renders a table of availability periods', async () => {
    const jsx = await AvailabilityCreatedEventsTable({
      siteId: mockSite.id,
    });

    render(jsx);

    expect(fetchAvailabilityCreatedEventsMock).toHaveBeenCalledWith(
      mockSite.id,
    );
    expect(screen.getByRole('table')).toBeInTheDocument();

    const columns = ['Dates', 'Days', 'Services', 'Session type'];
    columns.forEach(column => {
      screen.getByRole('columnheader', { name: column });
    });

    expect(screen.getAllByRole('row')).toHaveLength(5);

    expect(
      screen.getByRole('row', {
        name: '1 Jan 2024 - 28 Feb 2024 Mon, Tue RSV Adult Weekly repeating',
      }),
    );

    expect(
      screen.getByRole('row', {
        name: '1 Jan 2025 Wed RSV Adult Single date',
      }),
    );

    expect(
      screen.getByRole('row', {
        name: '1 Mar 2024 - 30 Apr 2024 All RSV Adult Weekly repeating',
      }),
    );

    expect(
      screen.getByRole('row', {
        name: '16 Feb 2025 Sun RSV Adult Single date',
      }),
    );
  });
});
