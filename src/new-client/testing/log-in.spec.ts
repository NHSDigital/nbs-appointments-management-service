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

  // await page.waitForURL('/', { waitUntil: 'domcontentloaded' });

  // await expect(
  //   page.getByRole('heading', { name: 'Appointment Management Service' }),
  // ).toBeVisible();
  await expect(
    page.getByRole('heading', { name: 'Choose a site' }),
  ).toBeVisible();
});
