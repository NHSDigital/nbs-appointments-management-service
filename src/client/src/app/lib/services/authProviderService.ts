import { AuthProvider } from '@types';

export const getProviders = (): AuthProvider[] => {
  return JSON.parse(process.env.AUTH_PROVIDERS ?? '');
};

export const getProvider = (provider: string): AuthProvider | undefined => {
  const providers = getProviders();
  return providers.find(p => p.NAME === provider);
};
