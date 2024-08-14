import { render, screen } from '@testing-library/react';
import { NhsHeader } from '@components/nhs-header';

jest.mock('@components/nhs-header-log-in', () => {
  const MockNhsHeaderLogIn = () => {
    return <button type="submit">log in</button>;
  };
  return MockNhsHeaderLogIn;
});

jest.mock('@components/nhs-header-log-out', () => {
  const MockNhsHeaderLogOut = () => {
    return <button type="submit">log out</button>;
  };
  return MockNhsHeaderLogOut;
});

describe('NhsHeader', () => {
  it('renders without email address and logout button when it is undefined', async () => {
    render(<NhsHeader />);
    expect(screen.queryByRole('button', { name: 'log out' })).toBeNull();
  });

  it('renders email address and logout button', async () => {
    render(<NhsHeader userEmail="test@test.com" />);
    expect(screen.queryByRole('button', { name: 'log out' })).toBeVisible();
    expect(screen.queryByText('test@test.com')).toBeVisible();
  });
});
