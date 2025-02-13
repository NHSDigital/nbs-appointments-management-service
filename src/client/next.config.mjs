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
        destination: process.env.CLIENT_BASE_PATH,
        permanent: true,
      },
    ];
  },
};

export default nextConfig;
