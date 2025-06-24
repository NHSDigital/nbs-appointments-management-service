import { SitesPage } from './sites-page';
import { render, screen, within } from '@testing-library/react';
import { mockSites } from '@testing/data';

describe('Sites Page', () => {
  it('should render the home page', () => {
    render(<SitesPage sites={mockSites} />);

    const rows = screen.getAllByRole('row');
    const dataRows = rows.slice(1);

    expect(dataRows).toHaveLength(4);

    const firstRow = dataRows[0];
    expect(within(firstRow).getByText('Site Alpha')).toBeInTheDocument();
    expect(within(firstRow).getByText('ICB1')).toBeInTheDocument();
    expect(within(firstRow).getByText('1001')).toBeInTheDocument();
    expect(
      within(firstRow).getByRole('link', { name: `View ${mockSites[0].name}` }),
    ).toHaveAttribute('href', `/site/34e990af-5dc9-43a6-8895-b9123216d699`);

    const secondRow = dataRows[1];
    expect(within(secondRow).getByText('Site Beta')).toBeInTheDocument();
    expect(within(secondRow).getByText('ICB2')).toBeInTheDocument();
    expect(within(secondRow).getByText('1002')).toBeInTheDocument();
    expect(
      within(secondRow).getByRole('link', {
        name: `View ${mockSites[1].name}`,
      }),
    ).toHaveAttribute('href', `/site/95e4ca69-da15-45f5-9ec7-6b2ea50f07c8`);

    const thirdRow = dataRows[2];
    expect(within(thirdRow).getByText('Site Delta')).toBeInTheDocument();
    expect(within(thirdRow).getByText('ICB4')).toBeInTheDocument();
    expect(within(thirdRow).getByText('1004')).toBeInTheDocument();
    expect(
      within(thirdRow).getByRole('link', { name: `View ${mockSites[3].name}` }),
    ).toHaveAttribute('href', `/site/90a9c1f2-83d0-4c40-9c7c-080d91c56e79`);

    const fourthRow = dataRows[3];
    expect(within(fourthRow).getByText('Site Gamma')).toBeInTheDocument();
    expect(within(fourthRow).getByText('ICB3')).toBeInTheDocument();
    expect(within(fourthRow).getByText('1003')).toBeInTheDocument();
    expect(
      within(fourthRow).getByRole('link', {
        name: `View ${mockSites[2].name}`,
      }),
    ).toHaveAttribute('href', `/site/d79bec60-8968-4101-b553-67dec04e1019`);
  });
});
