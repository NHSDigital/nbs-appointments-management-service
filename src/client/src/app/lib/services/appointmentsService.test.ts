import { cancelAppointment } from './appointmentsService';
import { appointmentsApi } from '@services/api/appointmentsApi';

jest.mock('@services/api/appointmentsApi', () => ({
  appointmentsApi: {
    post: jest.fn(),
  },
}));

jest.mock('@services/api/appointmentsApi', () => ({
  appointmentsApi: { post: jest.fn() },
}));

describe('Appointments Service', () => {
  const reference = 'REF123';
  const site = 'SITE001';
  const reason = 'Patient no longer available';

  afterEach(() => {
    jest.clearAllMocks();
  });

  it('calls the correct API endpoint with expected parameters', async () => {
    (appointmentsApi.post as jest.Mock).mockResolvedValue({ success: true });

    await cancelAppointment(reference, site, reason);

    expect(appointmentsApi.post).toHaveBeenCalledWith(
      `booking/${reference}/cancel?site=${site}&cancellationReason=${reason}`,
    );
  });
});
