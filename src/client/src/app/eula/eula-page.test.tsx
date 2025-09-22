import { screen, waitFor } from '@testing-library/react';
import { EulaVersion, ServerActionResult } from '@types';
import render from '@testing/render';
import EulaPage from './page';
import { fetchEula, acceptEula } from '@services/appointmentsService';
import { useRouter } from 'next/navigation';
import asServerActionResult from '@testing/asServerActionResult';

jest.mock('@services/appointmentsService');
const fetchEulaMock = fetchEula as jest.Mock<
  Promise<ServerActionResult<EulaVersion>>
>;
const acceptEulaMock = acceptEula as jest.Mock;

jest.mock('next/navigation');
const mockUseRouter = useRouter as jest.Mock;
const mockPush = jest.fn();

describe('EULA page', () => {
  beforeEach(() => {
    fetchEulaMock.mockResolvedValue(
      asServerActionResult({ versionDate: '2021-01-01' }),
    );
    acceptEulaMock.mockResolvedValue(asServerActionResult(undefined));

    mockUseRouter.mockReturnValue({
      push: mockPush,
    });
  });

  it('renders', async () => {
    const jsx = await EulaPage();
    render(jsx);

    expect(
      screen.getByRole('heading', {
        name: 'Agree to the terms of use',
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

  it('links to full terms of use for MYA', async () => {
    const jsx = await EulaPage();
    render(jsx);

    const termsOfUseLink = screen.getByRole('link', {
      name: 'Read the full terms of use for Manage Your Appointments',
    });

    expect(termsOfUseLink).toHaveAttribute(
      'href',
      'https://digital.nhs.uk/services/vaccinations-national-booking-service/terms-of-use',
    );
    expect(termsOfUseLink).toHaveAttribute('target', '_blank');
  });
});
