import { defineConfig, devices } from '@playwright/test';

/**
 * Read environment variables from file.
 * https://github.com/motdotla/dotenv
 */
// import dotenv from 'dotenv';
// dotenv.config({ path: path.resolve(__dirname, '.env') });

/**
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig({
  testDir: './testing',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  reporter: [['html', { outputFolder: 'testing/playwright-report' }]],
  outputDir: 'testing/playwright-report',

  // TODO: Playwright init defaults to 0 retries locally.
  // Let's try this for a bit with the goal of eliminating flakiness, but add in retries if we have to.
  retries: process.env.CI ? 2 : 0,
  // TODO: Playwright init defaults to opt out of parallel tests on CI. We should confirm if we agree with that
  workers: process.env.CI ? 1 : undefined,
  /* Reporter to use. See https://playwright.dev/docs/test-reporters */

  /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
  use: {
    baseURL: 'http://127.0.0.1:3000',
    trace: 'on-first-retry',
  },

  /* Configure projects for major browsers */
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    // TODO: Test with multiple browsers in CI, or maybe just on an ad hoc basis
    // Mobile Safari, firefox, edge, Chrome etc.
    // {
    //   name: 'Mobile Chrome',
    //   use: { ...devices['Pixel 5'] },
    // },
  ],

  // Builds and runs the NextJS app
  // If the app is already running on port 3000 this will do nothing
  webServer: [
    {
      command: 'npm run build && npm run start',
      url: 'http://127.0.0.1:3000',
      reuseExistingServer: !process.env.CI,
    },
    // TODO: Also start dotnet and docker services if not already running
    // {
    //   command: 'cd ../api && func start',
    //   port: 7071,
    //   reuseExistingServer: !process.env.CI,
    // },
    // {
    //   command: 'cd ../../ && docker-compose -up',
    //   url: 'https://127.0.0.1:8081/_explorer/index.html',
    //   ignoreHTTPSErrors: true,
    //   timeout: 1000 * 60 * 3,
    //   reuseExistingServer: !process.env.CI,
    // },
  ],
});
