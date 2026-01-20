/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  output: 'standalone',
  env: {
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000',
  },
  // Ensure standalone output includes all necessary files
  experimental: {
    outputFileTracingIncludes: {
      '/': ['./public/**/*'],
    },
  },
}

module.exports = nextConfig
