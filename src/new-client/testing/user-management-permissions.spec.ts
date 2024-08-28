import { test, expect } from '@playwright/test';
import env from './testEnvironment';

const {
  TEST_USER_1_USERNAME,
  TEST_USER_1_PASSWORD,
  TEST_USER_2_USERNAME,
  TEST_USER_2_PASSWORD,
  TEST_USER_3_USERNAME,
  TEST_USER_3_PASSWORD,
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

  await expect(
    page.getByRole('link', { name: 'Assign staff roles' }),
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

  await expect(page.getByRole('link', { name: 'Edit' })).toHaveCount(5);
});

test('Navigating straight to the user management page works as expected', async ({
  page,
}) => {
  await page.goto('/');
  await page.getByRole('button', { name: 'Log In' }).click();
  await page.getByLabel('Username').fill(TEST_USER_1_USERNAME);
  await page.getByLabel('Password').fill(TEST_USER_1_PASSWORD);
  await page.getByLabel('Password').press('Enter');

  await page.goto('/site/1000/users');
  await expect(
    page.getByRole('heading', { name: 'Manage Staff Roles' }),
  ).toBeVisible();
});

test('Navigating straight to the user management page displays an appropriate error if the permission is missing', async ({
  page,
}) => {
  await page.goto('/');
  await page.getByRole('button', { name: 'Log In' }).click();
  await page.getByLabel('Username').fill(TEST_USER_3_USERNAME);
  await page.getByLabel('Password').fill(TEST_USER_3_PASSWORD);
  await page.getByLabel('Password').press('Enter');

  await page.goto('/site/1000/users');

  await expect(
    page.getByRole('columnheader', { name: 'Email' }),
  ).not.toBeVisible();

  await expect(
    page.getByText('Forbidden: You lack the necessary permissions'),
  ).toBeVisible();

  await page.goto('/site/1000/users/manage');

  await expect(
    page.getByText('Set the details and roles of a new user'),
  ).not.toBeVisible();

  await expect(
    page.getByText('Forbidden: You lack the necessary permissions'),
  ).toBeVisible();
});

test('permissions are applied per site', async ({ page }) => {
  await page.goto('/');

  await expect(page.getByRole('button', { name: 'Log In' })).toBeVisible();

  await page.getByRole('button', { name: 'Log In' }).click();

  await page.getByLabel('Username').fill(TEST_USER_2_USERNAME);
  await page.getByLabel('Password').fill(TEST_USER_2_PASSWORD);

  await page.getByLabel('Password').press('Enter');

  // First check Edit column exists at Church Lane
  await page
    .getByRole('link', {
      name: 'Church Lane Surgery',
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

  // Then check it does NOT exist at Robin Lane
  await page
    .getByRole('link', {
      name: 'Home',
    })
    .click();

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
