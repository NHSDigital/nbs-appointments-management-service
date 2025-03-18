import render from '@testing/render';
import { screen, waitFor } from '@testing-library/react';
import CookieConsentForm from './cookie-consent-form';
import { setCookieConsent } from '@services/cookiesService';
import { useRouter } from 'next/navigation';

jest.mock('@services/cookiesService');
const mockSetCookieConsent = setCookieConsent as jest.Mock;

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockPush = jest.fn();

describe('Cookie consent form', () => {
  beforeEach(() => {
    mockUseRouter.mockReturnValue({
      push: mockPush,
    });
  });

  it('renders', () => {
    render(<CookieConsentForm />);

    expect(
      screen.getByRole('heading', {
        name: 'Tell us if we can use analytics cookies',
      }),
    ).toBeInTheDocument();
  });

  it('can update cookie acceptance preferences', async () => {
    const { user } = render(
      <CookieConsentForm consentCookie={{ consented: true, version: 1 }} />,
    );

    expect(
      screen.getByRole('radio', {
        name: 'Use cookies to measure my website use',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('radio', {
        name: 'Use cookies to measure my website use',
      }),
    ).toBeChecked();

    await user.click(
      screen.getByRole('radio', {
        name: 'Do not use cookies to measure my website use',
      }),
    );

    expect(
      screen.getByRole('radio', {
        name: 'Use cookies to measure my website use',
      }),
    ).not.toBeChecked();
    expect(
      screen.getByRole('radio', {
        name: 'Do not use cookies to measure my website use',
      }),
    ).toBeChecked();

    await user.click(
      screen.getByRole('button', { name: 'Save my cookie settings' }),
    );

    await waitFor(() => {
      expect(mockSetCookieConsent).toHaveBeenCalledWith(false);
      expect(mockPush).toHaveBeenCalledWith('/sites');
    });
  });

  it('defaults to non-consent if the user has not previously set preferences', () => {
    render(<CookieConsentForm consentCookie={undefined} />);

    expect(
      screen.getByRole('radio', {
        name: 'Use cookies to measure my website use',
      }),
    ).not.toBeChecked();
    expect(
      screen.getByRole('radio', {
        name: 'Do not use cookies to measure my website use',
      }),
    ).toBeChecked();
  });

  it('populates the form with current preferences - non consent', () => {
    render(
      <CookieConsentForm consentCookie={{ consented: false, version: 1 }} />,
    );

    expect(
      screen.getByRole('radio', {
        name: 'Use cookies to measure my website use',
      }),
    ).not.toBeChecked();
    expect(
      screen.getByRole('radio', {
        name: 'Do not use cookies to measure my website use',
      }),
    ).toBeChecked();
  });

  it('populates the form with current preferences - consent', () => {
    render(
      <CookieConsentForm consentCookie={{ consented: true, version: 1 }} />,
    );

    expect(
      screen.getByRole('radio', {
        name: 'Use cookies to measure my website use',
      }),
    ).toBeChecked();
    expect(
      screen.getByRole('radio', {
        name: 'Do not use cookies to measure my website use',
      }),
    ).not.toBeChecked();
  });
});
