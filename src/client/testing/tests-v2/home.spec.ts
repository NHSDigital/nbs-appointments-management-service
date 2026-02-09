import { test, expect } from '../fixtures-v2';
import { buildE2ETestSite } from '@e2etests/data';

test.describe.configure({ mode: 'serial' });

//test.describe('Home Page Access Control', () => {

  test('A user loads home page, only sites with same scope are loaded', async ({ 
    page, 
    setUpSingleSite 
  }) => {
    // We create a primary site and a secondary "restricted" site
    const { testId, additionalUserIds } = await setUpSingleSite({
      roles: ['canned:availability-manager'],
      additionalUsers: [{ roles: ['canned:availability-manager'] }] 
    });

    const site1 = buildE2ETestSite(testId);
    const site2Id = additionalUserIds.get('0')!; 
    const site2 = buildE2ETestSite(site2Id);

    // Assertions: site1 visible, site2 hidden (different scope)
    await expect(page.getByRole('cell').filter({ hasText: site1.name })).toBeVisible();
    await expect(page.getByRole('cell').filter({ hasText: site2.name })).not.toBeVisible();
  });

  test('An admin user loads home pasge, all sites are loaded', async ({ 
    setUpSingleSite,
    page 
  }) => {
    // Using multiple manager roles to simulate high-level access
    const { testId } = await setUpSingleSite({
      roles: [
        'canned:availability-manager', 
        'canned:user-manager', 
        'canned:site-details-manager'
      ]
    });

    const site1 = buildE2ETestSite(testId);
    await expect(page.getByRole('cell').filter({ hasText: site1.name })).toBeVisible();
  });

  test('A user searches for a site, site list is filtered', async ({ 
    page, 
    setUpSingleSite 
  }) => {
    const targetName = 'Robin Lane Medical Centre';
    const { testId } = await setUpSingleSite({
      siteConfig: { name: targetName }
    });
    
    // The fixture logs us in and selects the site. 
    // To test the home page search, we may need to navigate back to the list.
    await page.goto('/'); 

    const searchInput = page.getByRole('textbox', {
      name: 'Search active sites by name or ODS code',
    });
    
    await searchInput.fill('Church'); // Searching for something else
    await page.getByRole('button', { name: 'Search' }).click();

    await expect(page.getByRole('cell', { name: targetName })).not.toBeVisible();
  });
//});