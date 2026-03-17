import { test, expect } from '../fixtures-v2';

test.describe.configure({ mode: 'serial' });

test('A user loads home page, only sites with same scope are loaded', async ({
  page,
  setup,
}) => {
  const { site, additionalUserData } = await setup({
    roles: ['canned:availability-manager'],
    additionalUsers: [{ roles: ['canned:availability-manager'] }],
  });

  const additionalUserOne = additionalUserData.get('0');

  if (additionalUserOne?.site === undefined) {
    throw new Error('Expected Site to be defined in additionalUserData');
  }

  await page.goto('/');

  await expect(
    page.getByRole('cell').filter({ hasText: additionalUserOne.site.name }),
  ).not.toBeVisible();
  await expect(
    page.getByRole('cell').filter({ hasText: site.name }),
  ).toBeVisible();
});

test('An admin user loads home page, all sites are loaded', async ({
  setup,
  page,
}) => {
  // Create sites and skip selection so we land on the list
  const { site, additionalUserData } = await setup({
    roles: [
      'system:admin-user', // Use the correctly typed system role
      'canned:availability-manager',
      'canned:user-manager',
    ],
    additionalUsers: [{ roles: ['canned:availability-manager'] }],
    skipSiteSelection: true,
  });

  const additionalUserOne = additionalUserData.get('0');

  if (additionalUserOne?.site === undefined) {
    throw new Error('Expected Site to be defined in additionalUserData');
  }

  await expect(
    page.getByRole('cell', { name: site.name, exact: true }),
  ).toBeVisible();
  await expect(
    page.getByRole('cell', { name: additionalUserOne.site.name, exact: true }),
  ).toBeVisible();
});

test('A user loads home page and searches for a site, site list is filtered', async ({
  page,
  setup,
}) => {
  const targetName = 'Robin Lane Medical Centre';
  const otherSiteName = 'Church Lane Pharmacy';

  await setup({
    siteConfig: { name: targetName },
    skipSiteSelection: true, // Stay on Site Selection page to use the search bar
  }).then(async () => {
    // Verify the list is populated before we filter it
    await expect(
      page.getByRole('cell', { name: otherSiteName }),
    ).not.toBeVisible();
    await expect(page.getByText(targetName)).toBeVisible();

    const searchInput = page.getByRole('textbox', {
      name: 'Search active sites by name or ODS code',
    });

    // Perform Search
    await searchInput.fill('Church');
    const searchButton = page.getByRole('button', { name: 'Search' });
    await searchButton.click();

    // Final State Check
    // After searching for "Church", Robin Lane should be filtered out
    await expect(
      page.getByRole('cell', { name: targetName }),
    ).not.toBeVisible();
  });
});
