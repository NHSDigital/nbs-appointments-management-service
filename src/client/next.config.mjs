/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  env: {
    BUILD_NUMBER: process.env.BUILD_BUILDNUMBER ?? '',
  },
  output: 'standalone'
};

export default nextConfig;
