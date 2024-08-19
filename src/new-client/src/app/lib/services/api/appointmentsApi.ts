import Client from '@services/api/client';

export const appointmentsApi = new Client({
  baseUrl: `${process.env.NBS_API_BASE_URL}/api`,
});
