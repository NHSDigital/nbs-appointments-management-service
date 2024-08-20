import { test, expect } from '@playwright/test';
import env from './testEnvironment';

const { TEST_USER_USERNAME, TEST_USER_PASSWORD } = env;

test('logs in', async ({ page }) => {
  await page.goto('/');

  await expect(page.getByRole('button', { name: 'Log In' })).toBeVisible();

  await page.getByRole('button', { name: 'Log In' }).click();

  await page.getByLabel('Username').fill(TEST_USER_USERNAME);
  await page.getByLabel('Password').fill(TEST_USER_PASSWORD);

  await page.getByLabel('Password').press('Enter');

  await expect(
    page.getByRole('heading', { name: 'Choose a site' }),
  ).toBeVisible();

  await expect(page.getByRole('button', { name: 'Log Out' })).toBeVisible();
});

test('logs out', async ({ page }) => {
  await page.goto('/');
  await page.getByRole('button', { name: 'Log In' }).click();
  await page.getByLabel('Username').fill(TEST_USER_USERNAME);
  await page.getByLabel('Password').fill(TEST_USER_PASSWORD);
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
  await expect(page.getByRole('button', { name: 'Log In' })).toBeVisible();
});
