import { test, expect } from '../fixtures-v2';
import { SiteSelectionPage } from '@e2etests/page-objects';

test.describe.configure({ mode: 'serial' });
  
test('User visits the site origin, signs in and see the Site Selection menu', async ({ 
  page,
  setUpSingleSite 
}) => {
  await setUpSingleSite({ skipSiteSelection: true });

  // Since skipSiteSelection is true, sitePage is undefined, 
  // but the browser is left on the Site Selection page.
  const selectionPage = new SiteSelectionPage(page);
  
  await expect(selectionPage.title).toBeVisible();
  // Access the logOutButton via the header property defined in MYALayout
  await expect(selectionPage.header.logOutButton).toBeVisible();
});

test('User visits the site origin, signs in, then signs out again', async ({ 
  page, 
  setUpSingleSite 
}) => {
  await setUpSingleSite({ skipSiteSelection: true });
  
  const selectionPage = new SiteSelectionPage(page);

  await expect(selectionPage.header.logOutButton).toBeVisible();
  await selectionPage.header.logOutButton.click();

  await page.waitForURL('**/login');

  await expect(
    page.getByRole('heading', { name: 'Manage your appointments' }),
  ).toBeVisible();

  await expect(
    page.getByText(
      'You are currently not signed in. You must sign in to access this service.',
    ),
  ).toBeVisible();
});

test('Users with no roles at any site but valid auth credentials can still sign in', async ({ 
  page, 
  setUpSingleSite 
}) => {
  // We pass an empty array to roles to simulate a user with no permissions
  await setUpSingleSite({ 
    roles: [], 
    skipSiteSelection: true 
  });

  const selectionPage = new SiteSelectionPage(page);
  await expect(selectionPage.noSitesMessage).toBeVisible();
});