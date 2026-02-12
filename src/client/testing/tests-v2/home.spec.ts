import { test, expect } from '../fixtures-v2';
import { buildE2ETestSite } from '@e2etests/data';

test.describe.configure({ mode: 'serial' });

test('A user loads home page, only sites with same scope are loaded', async ({ 
  page, 
  setUpSingleSite 
}) => {
  const { testId, additionalUserIds } = await setUpSingleSite({
    roles: ['canned:availability-manager'],
    additionalUsers: [{ roles: ['canned:availability-manager'] }] 
  });

  const site1 = buildE2ETestSite(testId);
  const site2Id = additionalUserIds.get('0');

  if (site2Id === undefined) {
      throw new Error('Expected site2Id to be defined in additionalUserIds');
  }

  const site2 = buildE2ETestSite(site2Id);

  await page.goto('/');

  await expect(page.getByRole('cell').filter({ hasText: site2.name })).not.toBeVisible();
  await expect(page.getByRole('cell').filter({ hasText: site1.name })).toBeVisible();
});

test('An admin user loads home page, all sites are loaded', async ({ 
  setUpSingleSite,
  page 
}) => {
  // Create sites and skip selection so we land on the list
  const { testId, additionalUserIds } = await setUpSingleSite({
    roles: [
      'system:admin-user', // Use the correctly typed system role
      'canned:availability-manager', 
      'canned:user-manager', 
    ],
    additionalUsers: [{ roles: ['canned:availability-manager'] }],
    skipSiteSelection: true
  });

  const site1 = buildE2ETestSite(testId);
  const site2Id = additionalUserIds.get('0');

  if (site2Id === undefined) {
      throw new Error('Expected site2Id to be defined in additionalUserIds');
  }

  const site2 = buildE2ETestSite(site2Id);

  await expect(page.getByRole('cell', { name: site1.name, exact: true })).toBeVisible();
  await expect(page.getByRole('cell', { name: site2.name, exact: true })).toBeVisible();
});

test('A user loads home page and searches for a site, site list is filtered', async ({ 
  page, 
  setUpSingleSite 
}) => {
  const targetName = 'Robin Lane Medical Centre';
  const otherSiteName = 'Church Lane Pharmacy';

  await setUpSingleSite({
    siteConfig: { name: targetName },
    skipSiteSelection: true // Stay on Site Selection page to use the search bar
  }).then(async () => {
    // Verify the list is populated before we filter it
    await expect(page.getByRole('cell', { name: otherSiteName })).not.toBeVisible();
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
    await expect(page.getByRole('cell', { name: targetName })).not.toBeVisible();
  });
});