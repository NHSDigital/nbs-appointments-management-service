/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  env: {
    BUILD_NUMBER: process.env.BUILD_BUILDNUMBER ?? '',
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
        source: '/',
        basePath: true,
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
