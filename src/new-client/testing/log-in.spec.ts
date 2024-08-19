import { test, expect } from '@playwright/test';

// TODO: Get this from .env
const config = {
  testUser: {
    username: 'CC',
    password: '1234abc',
  },
};

test('Loads the site', async ({ page }) => {
  await page.goto('/');

  await expect(page).toHaveTitle('Appointment Management Service');
});

test('Has a log in link', async ({ page }) => {
  await page.goto('/');

  await expect(page.getByRole('button', { name: 'Log In' })).toBeVisible();
});

test('logs in', async ({ page }) => {
  await page.goto('/');

  await expect(page.getByRole('button', { name: 'Log In' })).toBeVisible();

  await page.getByRole('button', { name: 'Log In' }).click();

  await page.getByLabel('Username').fill(config.testUser.username);
  await page.getByLabel('Password').fill(config.testUser.password);

  await page.getByLabel('Password').press('Enter');

  await expect(
    page.getByRole('heading', { name: 'Choose a site' }),
  ).toBeVisible();
});

test('logs out', async ({ page }) => {
  await page.goto('/');
  await page.getByRole('button', { name: 'Log In' }).click();
  await page.getByLabel('Username').fill(config.testUser.username);
  await page.getByLabel('Password').fill(config.testUser.password);
  await page.getByLabel('Password').press('Enter');

  await expect(
    page.getByRole('heading', { name: 'Choose a site' }),
  ).toBeVisible();

  await expect(page.getByRole('button', { name: 'Log Out' })).toBeVisible();
  await page.getByRole('button', { name: 'Log Out' }).click();

  await expect(
    page.getByRole('heading', { name: 'You cannot access this site' }),
  ).toBeVisible();
  await expect(
    page.getByText(
      'You are currently not signed in. To use this site, please sign in.',
    ),
  ).toBeVisible();
});

test('Creates a token cookie upon sign in', async ({ browser }) => {
  const context = await browser.newContext();
  const page = await context.newPage();

  await page.goto('/');
  await page.getByRole('button', { name: 'Log In' }).click();
  await page.getByLabel('Username').fill(config.testUser.username);
  await page.getByLabel('Password').fill(config.testUser.password);
  await page.getByLabel('Password').press('Enter');

  await expect(
    page.getByRole('heading', { name: 'Choose a site' }),
  ).toBeVisible();

  const cookies = await context.cookies();
  expect(
    cookies.find(c => c.name === 'token' && c.value.startsWith('ey')),
  ).not.toBeUndefined();
});
