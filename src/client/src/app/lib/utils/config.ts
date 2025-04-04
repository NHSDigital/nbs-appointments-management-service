'use server';

export async function getConfig() {
  return {
    API_URL: process.env.API_URL,
    BUILD_NUMBER: process.env.BUILD_NUMBER,
    NODE_ENV: process.env.NODE_ENV,
    SENTRY_DSN: process.env.SENTRY_DSN,
    SENTRY_ENVIRONMENT: process.env.SENTRY_ENVIRONMENT,
    SENTRY_RELEASE: process.env.SENTRY_RELEASE,
    SENTRY_SAMPLE_RATE: process.env.SENTRY_SAMPLE_RATE,
  };
}
