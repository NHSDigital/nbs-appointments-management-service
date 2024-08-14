import { fetchUserProfile } from '@services/appointmentsService';
import Home from './page';
import { render, screen } from '@testing-library/react';

jest.mock('@services/appointmentsService');
const fetchUserProfileMock = fetchUserProfile as jest.Mock;

describe('Home Page', () => {
  it('should render the home page', async () => {
    fetchUserProfileMock.mockResolvedValue({
      emailAddress: 'test@test.com',
      availableSites: [],
    });

    const jsx = await Home();
    render(jsx);
    expect(screen.getByText('Appointment Management Service')).toBeVisible();
  });
});
