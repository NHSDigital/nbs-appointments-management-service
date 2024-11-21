import render from '@testing/render';
import { screen } from '@testing-library/react';
import { CreateAvailabilityPage } from './create-availability-page';
import { mockAvailabilityCreatedEvents, mockSite } from '@testing/data';
import { fetchAvailabilityCreatedEvents } from '@services/appointmentsService';
import { AvailabilityCreatedEvent } from '@types';

jest.mock('@services/appointmentsService');
const fetchAvailabilityCreatedEventsMock =
  fetchAvailabilityCreatedEvents as jest.Mock<
    Promise<AvailabilityCreatedEvent[]>
  >;

describe('Create Availability Page', () => {
  beforeEach(() => {
    fetchAvailabilityCreatedEventsMock.mockResolvedValue(
      mockAvailabilityCreatedEvents,
    );
  });
  it('renders', async () => {
    const jsx = await CreateAvailabilityPage({
      site: mockSite,
    });

    render(jsx);

    expect(
      screen.getByText(
        "You can create availability with multiple days and repeating sessions, to accurately reflect your site's capacity.",
      ),
    ).toBeInTheDocument;
  });

  it('renders a table of availability periods', async () => {
    const jsx = await CreateAvailabilityPage({
      site: mockSite,
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
        name: '1 January 2024 - 28 February 2024 Mon, Tue RSV (Adult) Weekly repeating',
      }),
    );

    expect(
      screen.getByRole('row', {
        name: '1 January 2025 Wed RSV (Adult) Single date',
      }),
    );

    expect(
      screen.getByRole('row', {
        name: '1 March 2024 - 30 April 2024 Thu, Fri RSV (Adult) Weekly repeating',
      }),
    );

    expect(
      screen.getByRole('row', {
        name: '16 February 2025 Sun RSV (Adult) Single date',
      }),
    );
  });

  it('renders a button to create more availability periods', async () => {
    const jsx = await CreateAvailabilityPage({
      site: mockSite,
    });

    render(jsx);

    expect(
      screen.getByRole('button', { name: 'Create availablity' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Create availablity' }),
    ).toHaveAttribute(
      'href',
      `/site/${mockSite.id}/create-availability/wizard`,
    );
  });
});
