import { screen, waitFor } from '@testing-library/react';
import { EulaVersion } from '@types';
import render from '@testing/render';
import EulaPage from './page';
import { fetchEula, acceptEula } from '@services/appointmentsService';

jest.mock('@services/appointmentsService');
const fetchEulaMock = fetchEula as jest.Mock<Promise<EulaVersion>>;
const acceptEulaMock = acceptEula as jest.Mock;

describe('EULA page', () => {
  beforeEach(() => {
    fetchEulaMock.mockResolvedValue({ versionDate: '2021-01-01' });
  });

  it('renders', async () => {
    const jsx = await EulaPage();
    render(jsx);

    expect(
      screen.getByRole('heading', {
        name: 'Agree to the End User Licence Agreement',
      }),
    ).toBeInTheDocument();

    expect(fetchEulaMock).toHaveBeenCalled();
  });

  it('lets the user accept the EULA', async () => {
    const jsx = await EulaPage();
    const { user } = render(jsx);

    await user.click(
      screen.getByRole('button', { name: 'Accept and continue' }),
    );

    waitFor(() => {
      expect(acceptEulaMock).toHaveBeenCalledWith('2021-01-01');
    });
  });

  it('links to the privacy policy and terms of use ', async () => {
    const jsx = await EulaPage();
    render(jsx);

    const termsOfUseLink = screen.getByRole('link', { name: 'terms of use' });

    expect(termsOfUseLink).toHaveAttribute(
      'href',
      'https://digital.nhs.uk/coronavirus/vaccinations/booking-systems/terms-of-use',
    );
    expect(termsOfUseLink).toHaveAttribute('target', '_blank');

    const privacyPolicyLink = screen.getByRole('link', {
      name: 'privacy policy',
    });

    expect(privacyPolicyLink).toHaveAttribute(
      'href',
      'https://www.nhs.uk/our-policies/nbs-privacy-policy/',
    );
    expect(privacyPolicyLink).toHaveAttribute('target', '_blank');
  });
});
