/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  env: {
    BUILD_NUMBER: process.env.BUILD_NUMBER ?? '',
  },
  // TODO: This comes as standard in NextJS V15 so can be removed when we upgrade
  experimental: {
    instrumentationHook: true,
  },
  output: 'standalone',
  basePath: process.env.CLIENT_BASE_PATH,
  redirects: async () => {
    return [
      {
        source: '/',
        basePath: false,
        destination: `${process.env.CLIENT_BASE_PATH}/sites`,
        permanent: true,
      },
      {
        source: '/manage-your-appointments/api/:path*',
        destination: `${process.env.AUTH_HOST}/api/:path*`, // Ensure API calls go to backend
        permanent: true,
      },
    ];
  },
};

export default nextConfig;
