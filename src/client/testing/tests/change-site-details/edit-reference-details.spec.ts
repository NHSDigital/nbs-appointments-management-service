import { test, expect } from '../../fixtures';
import env from '../../testEnvironment';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import SiteSelectionPage from '../../page-objects/site-selection';
import SitePage from '../../page-objects/site';
import SiteDetailsPage from '../../page-objects/change-site-details-pages/site-details';
import EditReferenceDetailsPage from '../../page-objects/change-site-details-pages/edit-reference-details';

const { TEST_USERS } = env;

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let editReferenceDetailsPage: EditReferenceDetailsPage;
let siteDetailsPage: SiteDetailsPage;

// Annotate entire file as serial.
test.describe.configure({ mode: 'serial' });

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  editReferenceDetailsPage = new EditReferenceDetailsPage(page);
  siteDetailsPage = new SiteDetailsPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);
  await siteSelectionPage.selectSite('Church Lane Pharmacy');
  await sitePage.siteManagementCard.click();
  await page.waitForURL('**/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/details');
  await siteDetailsPage.editSiteReferenceDetailsButton.click();

  await page.waitForURL(
    '**/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/details/edit-reference-details',
  );
});

test('Clicking back mid-form does not save the changes', async ({ page }) => {
  await editReferenceDetailsPage.odsCodeInput.fill('ABC000032434543');
  await editReferenceDetailsPage.icbSelectInput.selectOption(
    'Integrated Care Board 1',
  );
  await editReferenceDetailsPage.regionSelectInput.selectOption('Region 1');
  await editReferenceDetailsPage.backLink.click();

  //verify the data is NOT present on the new details
  await page.waitForURL('**/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/details');

  await siteDetailsPage.verifyReferenceDetailsNotificationVisibility(false);

  //verify default state
  await siteDetailsPage.verifyDefaultReferenceDetailsOnPage();

  // Go back into edit UI to assert input is same as before
  await siteDetailsPage.editSiteReferenceDetailsButton.click();
  await page.waitForURL(
    '**/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/details/edit-reference-details',
  );

  await expect(editReferenceDetailsPage.odsCodeInput).toHaveValue('ABC02');
  await expect(editReferenceDetailsPage.icbSelectInput).toHaveValue('ICB2');
  await expect(editReferenceDetailsPage.regionSelectInput).toHaveValue('R2');

  await editReferenceDetailsPage.page.reload();
  await page.waitForURL(
    '**/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/details/edit-reference-details',
  );

  await expect(editReferenceDetailsPage.odsCodeInput).toHaveValue('ABC02');
  await expect(editReferenceDetailsPage.icbSelectInput).toHaveValue('ICB2');
  await expect(editReferenceDetailsPage.regionSelectInput).toHaveValue('R2');
});

test('Update reference details for a site, and verify present on the details page and in the new inputs', async ({
  page,
}) => {
  await editReferenceDetailsPage.odsCodeInput.fill('ABC000032434543');
  await editReferenceDetailsPage.icbSelectInput.selectOption(
    'Integrated Care Board 1',
  );
  await editReferenceDetailsPage.regionSelectInput.selectOption('Region 1');
  await editReferenceDetailsPage.saveAndContinueButton.click();

  //verify the data is present on the new details
  await page.waitForURL('**/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/details');

  await siteDetailsPage.verifyReferenceDetailsNotificationVisibility(true);

  await siteDetailsPage.verifyReferenceDetailsContent(
    'ABC000032434543',
    'Integrated Care Board 1',
    'Region 1',
  );

  // Go back into edit UI to assert new input is there
  await siteDetailsPage.editSiteReferenceDetailsButton.click();
  await page.waitForURL(
    '**/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/details/edit-reference-details',
  );

  await expect(editReferenceDetailsPage.odsCodeInput).toHaveValue(
    'ABC000032434543',
  );
  await expect(editReferenceDetailsPage.icbSelectInput).toHaveValue('ICB1');
  await expect(editReferenceDetailsPage.regionSelectInput).toHaveValue('R1');

  await editReferenceDetailsPage.page.reload();
  await page.waitForURL(
    '**/site/6877d86e-c2df-4def-8508-e1eccf0ea6be/details/edit-reference-details',
  );

  await expect(editReferenceDetailsPage.odsCodeInput).toHaveValue(
    'ABC000032434543',
  );
  await expect(editReferenceDetailsPage.icbSelectInput).toHaveValue('ICB1');
  await expect(editReferenceDetailsPage.regionSelectInput).toHaveValue('R1');
});
