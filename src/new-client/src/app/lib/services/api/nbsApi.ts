import Client from '@services/api/client';

export const nbsApi = new Client({
  baseUrl: `${process.env.NBS_API_BASE_URL}/api`,
});
