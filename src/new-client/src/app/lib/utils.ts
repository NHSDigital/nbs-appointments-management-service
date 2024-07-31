export const getEndpoint = (path: string): string =>
  `${process.env.NBS_API_BASE_URL}/api/${path}`;
