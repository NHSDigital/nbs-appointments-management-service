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
  it('renders', () => {
    render(<CreateAvailabilityPage site={mockSite} />);

    expect(
      screen.getByText(
        "You can create availability with multiple days and repeating sessions, to accurately reflect your site's capacity.",
      ),
    ).toBeInTheDocument;
  });

  it('renders a button to create more availability periods', () => {
    render(<CreateAvailabilityPage site={mockSite} />);

    expect(
      screen.getByRole('button', { name: 'Create availability' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'Create availability' }),
    ).toHaveAttribute(
      'href',
      `/site/${mockSite.id}/create-availability/wizard`,
    );
  });
});
