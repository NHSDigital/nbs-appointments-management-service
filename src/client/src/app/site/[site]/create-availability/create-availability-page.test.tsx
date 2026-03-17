import render from '@testing/render';
import { screen } from '@testing-library/react';
import { CreateAvailabilityPage } from './create-availability-page';
import { mockSite } from '@testing/data';

jest.mock('./availabilityCreatedEventsTable', () => {
  const MockAvailabilityCreatedEventsTable = () => {
    return <div>This is a mock table</div>;
  };
  return {
    AvailabilityCreatedEventsTable: MockAvailabilityCreatedEventsTable,
  };
});

describe('Create Availability Page', () => {
  it('renders a button to create more availability periods', () => {
    render(<CreateAvailabilityPage site={mockSite} cancelADateRange={false} />);

    expect(
      screen.getByRole('button', { name: 'Create new availability' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Create new availability' }),
    ).toHaveAttribute(
      'href',
      `/site/${mockSite.id}/create-availability/wizard`,
    );
  });

  it('renders a button to change availability', () => {
    render(<CreateAvailabilityPage site={mockSite} cancelADateRange={true} />);

    expect(
      screen.getByRole('button', { name: 'Change availability' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Change availability' }),
    ).toHaveAttribute('href', `/site/${mockSite.id}/change-availability`);
  });

  it('does not render a button to change availability', () => {
    render(<CreateAvailabilityPage site={mockSite} cancelADateRange={false} />);

    expect(
      screen.queryByRole('button', { name: 'Change availability' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('link', { name: 'Change availability' }),
    ).not.toBeInTheDocument();
  });

  it('renders the session history section with the correct legend and helper text', () => {
    render(<CreateAvailabilityPage site={mockSite} cancelADateRange={false} />);

    expect(
      screen.getByText('History of sessions you created'),
    ).toBeInTheDocument();

    expect(
      screen.getByText(/If you cancel a session, it will still show here/i),
    ).toBeInTheDocument();

    expect(
      screen.getByText('Any sessions that ended in the past are hiden.'),
    ).toBeInTheDocument();
  });

  it('renders the "View availability" link with the correct destination', () => {
    render(<CreateAvailabilityPage site={mockSite} cancelADateRange={false} />);

    const viewAvailabilityLink = screen.getByRole('link', {
      name: 'View availability',
    });

    expect(viewAvailabilityLink).toBeInTheDocument();
    expect(viewAvailabilityLink).toHaveAttribute(
      'href',
      `/site/${mockSite.id}/view-availability`,
    );
  });
});
