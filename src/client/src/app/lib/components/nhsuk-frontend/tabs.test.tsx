import { render, screen } from '@testing-library/react';
import { Tabs } from '@nhsuk-frontend-components';
import userEvent from '@testing-library/user-event';

const mockTabsChildren = [
  {
    isSelected: true,
    url: `/tab1`,
    tabTitle: 'tab1',
    content: 'Tab 1 content',
  },
  {
    isSelected: false,
    url: `/tab2`,
    tabTitle: 'tab2',
    content: 'Tab 2 content',
  },
];

describe('<Table />', () => {
  it('renders', () => {
    render(<Tabs>{mockTabsChildren}</Tabs>);

    const links = screen.getAllByRole('link');

    expect(links).toHaveLength(mockTabsChildren.length);
    expect(links[0]).toHaveTextContent(mockTabsChildren[0].tabTitle);
    expect(links[1]).toHaveTextContent(mockTabsChildren[1].tabTitle);
  });

  it('selecting second tab renders different content', async () => {
    render(<Tabs>{mockTabsChildren}</Tabs>);

    const tab2Link = screen.getByText(mockTabsChildren[1].tabTitle);

    expect(screen.getByText(mockTabsChildren[0].tabTitle)).toBeInTheDocument();
    await userEvent.click(tab2Link);
    expect(screen.getByText(mockTabsChildren[1].tabTitle)).toBeInTheDocument();
  });
});
