import { test, expect } from '@playwright/test';
import env from './testEnvironment';

const {
  TEST_USER_1_USERNAME,
  TEST_USER_1_PASSWORD,
  TEST_USER_2_USERNAME,
  TEST_USER_2_PASSWORD,
} = env;

test('A user with the appropriate permission can view other users at a site but not edit them', async ({
  page,
}) => {
  await page.goto('/');

  await expect(page.getByRole('button', { name: 'Log In' })).toBeVisible();

  await page.getByRole('button', { name: 'Log In' }).click();

  await page.getByLabel('Username').fill(TEST_USER_2_USERNAME);
  await page.getByLabel('Password').fill(TEST_USER_2_PASSWORD);

  await page.getByLabel('Password').press('Enter');

  await page
    .getByRole('link', {
      name: 'Robin Lane Medical Centre',
    })
    .click();

  await page
    .getByRole('link', {
      name: 'User Management',
    })
    .click();

  await expect(
    page.getByRole('columnheader', { name: 'Manage' }),
  ).not.toBeVisible();
});

test('A user with the appropriate permission can view other users at a site and also edit them', async ({
  page,
}) => {
  await page.goto('/');

  await expect(page.getByRole('button', { name: 'Log In' })).toBeVisible();

  await page.getByRole('button', { name: 'Log In' }).click();

  await page.getByLabel('Username').fill(TEST_USER_1_USERNAME);
  await page.getByLabel('Password').fill(TEST_USER_1_PASSWORD);

  await page.getByLabel('Password').press('Enter');

  await page
    .getByRole('link', {
      name: 'Robin Lane Medical Centre',
    })
    .click();

  await page
    .getByRole('link', {
      name: 'User Management',
    })
    .click();

  await expect(
    page.getByRole('columnheader', { name: 'Manage' }),
  ).toBeVisible();

  await expect(page.getByRole('link', { name: 'Edit' })).toHaveCount(4);
});
