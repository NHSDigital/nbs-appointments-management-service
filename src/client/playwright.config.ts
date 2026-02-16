import { defineConfig, devices } from '@playwright/test';
import dotenv from 'dotenv';
import path from 'path';

if (!process.env.CI) {
  process.env.PLAYWRIGHT_BROWSERS_PATH = '0';
}

dotenv.config({
  path: path.resolve(__dirname, 'testing', 'playwright.env'),
});

export default defineConfig({
  testDir: './testing',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  reporter: [
    ['junit', { outputFile: 'testing/e2e-junit-results.xml' }],
    ['html', { outputFolder: 'testing/playwright-report' }],
  ],
  outputDir: './test-artifacts',
  globalSetup: require.resolve('./testing/global-setup'),

  // TODO: Playwright init defaults to 0 retries locally.
  // Let's try this for a bit with the goal of eliminating flakiness, but add in retries if we have to.
  retries: process.env.CI ? 2 : 1,
  // TODO: Playwright init defaults to opt out of parallel tests on CI. We should confirm if we agree with that
  workers: process.env.CI ? 4 : 1,
  use: {
    baseURL: process.env.BASE_URL,
    trace: 'on-first-retry',
    screenshot: {
      mode: 'only-on-failure',
      fullPage: true,
    },
  },

  projects: [
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        // TODO: Remove all 3 of these security bypasses.
        // These are not necessary locally, but in the pipeline the ID server doesn't have a valid cert
        // This causes a the following error when the login button is clicked during a Playwright test
        // Error: page.waitForURL: net::ERR_CERT_AUTHORITY_INVALID
        // https://nhsd-jira.digital.nhs.uk/browse/APPT-1571
        ignoreHTTPSErrors: true,
        bypassCSP: true,
        launchOptions: {
          args: ['--disable-web-security', '--allow-insecure-localhost'],
        },
      },
    },
    {
      name: 'Microsoft Edge',
      use: {
        ...devices['Desktop Edge'],
        channel: 'msedge',
        ignoreHTTPSErrors: true,
        bypassCSP: true,
        launchOptions: {
          args: ['--disable-web-security', '--allow-insecure-localhost'],
        },
      },
    },
    {
      name: 'firefox',
      use: {
        ...devices['Desktop Firefox'],
        ignoreHTTPSErrors: true,
        bypassCSP: true,
        launchOptions: {
          args: ['--disable-web-security', '--allow-insecure-localhost'],
        },
      },
    },
    {
      name: 'webkit',
      use: {
        ...devices['Desktop Safari'],
        ignoreHTTPSErrors: true,
        bypassCSP: true,
        launchOptions: {
          args: [
            '--disable-web-security',
            '--no-sandbox',
          ],
          env: {
            ...process.env,
            // This is the "magic" for libcurl on Windows
            // It forces WebKit to use a more lenient SSL backend
            CURL_SSL_BACKEND: 'schannel', 
            WEBKIT_DISABLE_SANDBOX: '1',
          },
        },
      },
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
      // command: 'npm run build && npm run start',
      // url: process.env.BASE_URL,
      // reuseExistingServer: true,
      command: 'npm run build && npm run start',
      url: process.env.BASE_URL,
      reuseExistingServer: true,
      timeout: 120 * 1000, // Increase to 2 minutes
      stdout: 'ignore',    // Keep it quiet unless you are debugging the build
      stderr: 'pipe',
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
