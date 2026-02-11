import { test, expect } from '../fixtures-v2';
import { EulaConsentPage, SiteSelectionPage, LoginPage } from '@e2etests/page-objects';
import { buildE2ETestUser } from '@e2etests/data';

test.describe.configure({ mode: 'serial' });

const STALE_EULA_DATE: string = '2000-01-01';

test('A user with an out of date EULA consent version is prompted with the EULA consent page', async ({
  page,
  setUpSingleSite,
}) => {
  // 1. Set up the site/user. 
  // If your app triggers EULA for ALL new users, this is enough.
  // If it requires a specific configuration, pass it in siteConfig.
  await setUpSingleSite({ 
    skipSiteSelection: true,
    userConfig: { 
      // Use a valid date format that the C# converter can parse
      latestAcceptedEulaVersion: STALE_EULA_DATE
    }
  });

  const eulaPage = new EulaConsentPage(page);

  // 2. The fixture handles the login and should stop at /eula
  await expect(eulaPage.title).toBeVisible();

  // 3. Verify it redirects back to EULA
  await page.goto('/manage-your-appointments/sites');
  await expect(page).toHaveURL(/.*\/eula/);
});

test('A user with an out of date EULA version...', async ({
  page,
  setUpSingleSite,
}) => {
  // 1. Use the modified fixture (Safe because skipSiteSelection defaults to false elsewhere)
  const { testId } = await setUpSingleSite({ 
    skipSiteSelection: true,
    userConfig: { latestAcceptedEulaVersion: STALE_EULA_DATE } 
  });

  const eulaPage = new EulaConsentPage(page);
  const selectionPage = new SiteSelectionPage(page);

  // 2. Initial flow
  await page.waitForURL('**/eula');
  await eulaPage.acceptAndContinueButton.click();

  // 3. Logout
  await selectionPage.header.logOutButton.click();
  await page.waitForURL(/.*login|.*\//);

  // 4. Logout
  await selectionPage.header.logOutButton.click();
  
  // Clear cookies to kill the "sticky" session that is auto-logging you back in
  await page.context().clearCookies();

  // 5. Re-login
  const loginPage = new LoginPage(page);
  
  // Force navigate to the login page to ensure we aren't stuck on /sites
  await page.goto('/'); 

  // Use the login helper that worked in the fixture
  const mockOidcLoginPage = await loginPage.logInWithNhsMail();

  const user = buildE2ETestUser(testId);
  await mockOidcLoginPage.usernameField.fill(user.username);
  await mockOidcLoginPage.passwordField.fill(user.password);
  await mockOidcLoginPage.passwordField.press('Enter');

  // 6. Verify EULA is skipped
  // Since we already accepted it in Step 2, this should now hit /sites
  await page.waitForURL('**/sites');
  await expect(selectionPage.title).toBeVisible();
});