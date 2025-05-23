import {
  EditReferenceDetailsPage,
  OAuthLoginPage,
  RootPage,
  SiteDetailsPage,
  SitePage,
  SiteSelectionPage,
} from '@testing-page-objects';
import { test, expect } from '../../fixtures';
import { Site } from '@types';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let editReferenceDetailsPage: EditReferenceDetailsPage;
let siteDetailsPage: SiteDetailsPage;

let site: Site;

// Annotate entire file as serial.
test.describe.configure({ mode: 'serial' });

test.beforeEach(async ({ page, getTestSite }) => {
  site = getTestSite(2);
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  editReferenceDetailsPage = new EditReferenceDetailsPage(page);
  siteDetailsPage = new SiteDetailsPage(page, site);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite(site);
  await sitePage.siteManagementCard.click();
  await page.waitForURL(`**/site/${site.id}/details`);
  await siteDetailsPage.editSiteReferenceDetailsButton.click();

  await page.waitForURL(`**/site/${site.id}/details/edit-reference-details`);
});

test('Clicking back mid-form does not save the changes', async ({ page }) => {
  await editReferenceDetailsPage.odsCodeInput.fill('ABC000032434543');
  await editReferenceDetailsPage.icbSelectInput.selectOption(
    'Integrated Care Board 1',
  );
  await editReferenceDetailsPage.regionSelectInput.selectOption('Region 1');
  await editReferenceDetailsPage.backLink.click();

  //verify the data is NOT present on the new details
  await page.waitForURL(`**/site/${site.id}/details`);

  await siteDetailsPage.verifyReferenceDetailsNotificationVisibility(false);

  //verify default state
  await siteDetailsPage.verifySitePage();

  // Go back into edit UI to assert input is same as before
  await siteDetailsPage.editSiteReferenceDetailsButton.click();
  await page.waitForURL(`**/site/${site.id}/details/edit-reference-details`);

  await expect(editReferenceDetailsPage.odsCodeInput).toHaveValue('ABC02');
  await expect(editReferenceDetailsPage.icbSelectInput).toHaveValue('ICB2');
  await expect(editReferenceDetailsPage.regionSelectInput).toHaveValue('R2');

  await editReferenceDetailsPage.page.reload();
  await page.waitForURL(`**/site/${site.id}/details/edit-reference-details`);

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
  await page.waitForURL(`**/site/${site.id}/details`);

  await siteDetailsPage.verifyReferenceDetailsNotificationVisibility(true);

  await siteDetailsPage.verifyReferenceDetailsContent(
    'ABC000032434543',
    'Integrated Care Board 1',
    'Region 1',
  );

  // Go back into edit UI to assert new input is there
  await siteDetailsPage.editSiteReferenceDetailsButton.click();
  await page.waitForURL(`**/site/${site.id}/details/edit-reference-details`);

  await expect(editReferenceDetailsPage.odsCodeInput).toHaveValue(
    'ABC000032434543',
  );
  await expect(editReferenceDetailsPage.icbSelectInput).toHaveValue('ICB1');
  await expect(editReferenceDetailsPage.regionSelectInput).toHaveValue('R1');

  await editReferenceDetailsPage.page.reload();
  await page.waitForURL(`**/site/${site.id}/details/edit-reference-details`);

  await expect(editReferenceDetailsPage.odsCodeInput).toHaveValue(
    'ABC000032434543',
  );
  await expect(editReferenceDetailsPage.icbSelectInput).toHaveValue('ICB1');
  await expect(editReferenceDetailsPage.regionSelectInput).toHaveValue('R1');
});
