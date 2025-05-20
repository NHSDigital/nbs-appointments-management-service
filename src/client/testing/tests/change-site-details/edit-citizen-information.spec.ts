import {
  EditInformationForCitizensPage,
  OAuthLoginPage,
  RootPage,
  SiteDetailsPage,
  SitePage,
  SiteSelectionPage,
} from '@testing-page-objects';
import { test } from '../../fixtures';
import { Site } from '@types';

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let siteDetailsPage: SiteDetailsPage;
let editInformCitizen: EditInformationForCitizensPage;

let site: Site;

// Annotate entire file as serial.
test.describe.configure({ mode: 'serial' });

test.beforeEach(async ({ page, getTestSite }) => {
  site = getTestSite(2);
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  siteDetailsPage = new SiteDetailsPage(page, site);
  editInformCitizen = new EditInformationForCitizensPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn();
  await siteSelectionPage.selectSite(site);
  await sitePage.siteManagementCard.click();
  await page.waitForURL(`**/site/${site.id}/details`);
  await siteDetailsPage.editInformationCitizenButton.click();
  await page.waitForURL(
    `**/site/${site.id}/details/edit-information-for-citizens`,
  );
});

test('Update information for citizen', async ({ page }) => {
  await editInformCitizen.verifyInformationForCitizenPageDetails();
  await editInformCitizen.setInformationForCitizen('Test Automation');
  await editInformCitizen.save_Cancel_InformationForCitizen('Save');

  await page.waitForURL(`**/site/${site.id}/details`);

  await siteDetailsPage.verifyInformationSaved('Test Automation');
});

test('Verify information not saved when cancel button clicked', async () => {
  await editInformCitizen.setInformationForCitizen('Test Automation');
  await editInformCitizen.save_Cancel_InformationForCitizen('Save');
  await siteDetailsPage.editInformationCitizenButton.click();
  await editInformCitizen.setInformationForCitizen('Changed Information');
  await editInformCitizen.save_Cancel_InformationForCitizen('Cancel');
  await siteDetailsPage.verifyInformationNotSaved(
    'Test Automation',
    'Changed Information',
  );
});

test('Verify validation handling for information text field', async () => {
  await editInformCitizen.VerifyValidationMessage();
});
