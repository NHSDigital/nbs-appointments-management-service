import { test, expect } from '../../fixtures';
import env from '../../testEnvironment';
import RootPage from '../../page-objects/root';
import OAuthLoginPage from '../../page-objects/oauth';
import SiteSelectionPage from '../../page-objects/site-selection';
import SitePage from '../../page-objects/site';
import SiteManagementPage from '../../page-objects/site-management';
import SiteDetailsPage from '../../page-objects/site-details';
import EditInformationForCitizensPage from '../../page-objects/change-site-details-pages/edit-citizenship-information';

const { TEST_USERS } = env;

let rootPage: RootPage;
let oAuthPage: OAuthLoginPage;
let siteSelectionPage: SiteSelectionPage;
let sitePage: SitePage;
let siteDetailsPage: SiteDetailsPage;
let editInformCitizen: EditInformationForCitizensPage;

test.beforeEach(async ({ page }) => {
  rootPage = new RootPage(page);
  oAuthPage = new OAuthLoginPage(page);
  siteSelectionPage = new SiteSelectionPage(page);
  sitePage = new SitePage(page);
  siteDetailsPage = new SiteDetailsPage(page);
  editInformCitizen = new EditInformationForCitizensPage(page);

  await rootPage.goto();
  await rootPage.pageContentLogInButton.click();
  await oAuthPage.signIn(TEST_USERS.testUser1);
  await siteSelectionPage.selectSite('Church Lane Pharmacy');
  await sitePage.siteManagementCard.click();
  await page.waitForURL('**/site/ABC02/details');
  await siteDetailsPage.editInformationCitizenButton.click();
  await page.waitForURL('**/site/ABC02/details/edit-information-for-citizens');
});

test('Update information for citizen', async () => {
  await editInformCitizen.verifyInformationForCitizenPageDetails();
  await editInformCitizen.setInformationForCitizen('Test Automation');
  await editInformCitizen.save_Cancel_InformationForCitizen('Save');
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
