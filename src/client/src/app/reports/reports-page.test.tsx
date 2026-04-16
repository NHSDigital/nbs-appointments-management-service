import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { useRouter, useSearchParams } from 'next/navigation';
import { ReportsPage } from './reports-page';

jest.mock('next/navigation', () => ({
  useRouter: jest.fn(),
  useSearchParams: jest.fn(),
}));

describe('ReportsPage', () => {
  const mockPush = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (useRouter as jest.Mock).mockReturnValue({ push: mockPush });

    Object.defineProperty(document, 'referrer', {
      value: 'http://localhost/internal-path',
      configurable: true,
    });
  });

  it('navigates to the encoded returnUrl when Back is clicked', async () => {
    const complexUrl = '/daily-appointments?date=2026-04-15&page=1';

    (useSearchParams as jest.Mock).mockReturnValue({
      get: (key: string) => (key === 'returnUrl' ? complexUrl : null),
    });

    const user = userEvent.setup();
    render(<ReportsPage />);

    const backLink = screen.getByRole('link', { name: /Back/i });
    await user.click(backLink);

    expect(mockPush).toHaveBeenCalledWith(complexUrl);
  });

  it('defaults to /sites when no returnUrl is available', async () => {
    (useSearchParams as jest.Mock).mockReturnValue({
      get: () => null,
    });

    const user = userEvent.setup();
    render(<ReportsPage />);

    const backLink = screen.getByRole('link', { name: /Back/i });
    await user.click(backLink);

    expect(mockPush).toHaveBeenCalledWith('/sites');
  });
});
