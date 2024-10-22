import render from '@testing/render';
import { screen } from '@testing-library/react';
import { CreateAvailabilityPage } from './create-availability-page';
import { mockAvailabilityPeriods, mockSite } from '@testing/data';
import { fetchAvailabilityPeriods } from '@services/appointmentsService';
import { AvailabilityPeriod } from '@types';

jest.mock('@services/appointmentsService');
const fetchAvailabilityPeriodsMock = fetchAvailabilityPeriods as jest.Mock<
  Promise<AvailabilityPeriod[]>
>;
describe('Create Availability Page', () => {
  beforeEach(() => {
    fetchAvailabilityPeriodsMock.mockResolvedValue(mockAvailabilityPeriods);
  });

  it('renders', async () => {
    const jsx = await CreateAvailabilityPage({ site: mockSite });

    render(jsx);

    expect(
      screen.getByText(
        "You can create and edit availability periods with multiple days and repeating sessions, to accurately reflect your site's capacity.",
      ),
    ).toBeInTheDocument;
  });

  // TODO: Re-implement this once this page has gone back through design
  it.skip('renders a table of availability periods', async () => {
    const jsx = await CreateAvailabilityPage({ site: mockSite });

    render(jsx);

    expect(fetchAvailabilityPeriodsMock).toHaveBeenCalledWith(mockSite.id);
    expect(screen.getByRole('table')).toBeInTheDocument();

    const columns = ['Dates', 'Services', 'Status', 'Actions'];
    columns.forEach(column => {
      screen.getByRole('columnheader', { name: column });
    });

    expect(screen.getAllByRole('rowgroup')).toHaveLength(2);
    expect(
      screen.getByRole('cell', { name: '21 Apr 2025 - 15 Jun 2025' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('cell', { name: '15 Jun 2025 - 05 Oct 2025' }),
    ).toBeInTheDocument();
    expect(screen.getAllByRole('strong')).toHaveLength(2);
    expect(screen.getAllByRole('link', { name: 'Edit' })).toHaveLength(2);
  });

  it('renders a button to create more availability periods', async () => {
    const jsx = await CreateAvailabilityPage({ site: mockSite });

    render(jsx);

    expect(
      screen.getByRole('button', { name: 'Create availablity period' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Create availablity period' }),
    ).toHaveAttribute(
      'href',
      `/site/${mockSite.id}/create-availability/wizard`,
    );
  });
});
