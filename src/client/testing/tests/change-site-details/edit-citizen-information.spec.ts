import {
  EditInformationForCitizensPage,
  RootPage,
} from '@testing-page-objects';
import { test, expect } from '../../fixtures';

let put: EditInformationForCitizensPage;

test.describe.configure({ mode: 'serial' });

test.beforeEach(async ({ page, getTestSite }) => {
  put = await new RootPage(page)
    .logInWithNhsMail()
    .then(oAuthPage => oAuthPage.signIn())
    .then(siteSelectionPage => siteSelectionPage.selectSite(getTestSite(2)))
    .then(sitePage => sitePage.clickSiteDetailsCard())
    .then(siteDetailsPage =>
      siteDetailsPage.clickEditInformationForCitizensLink(),
    );
});

test(
  'A user updates the citizen information for a site',
  { tag: ['@affects:site2'] },
  async () => {
    await expect(put.title).toBeVisible();
    await expect(put.infoTextArea).toHaveText(
      'Mock information for citizens about site 2',
    );
    await expect(put.page.getByText(''));

    await put.infoTextArea.clear(); //();
    await put.infoTextArea.fill('New information for citizens about site 2');
    const siteDetailsPage = await put.saveCitizenInformation();

    const expectedNotification =
      'You have successfully updated the access needs for the current site.';
    await expect(
      siteDetailsPage.notificationBanner.getByText(expectedNotification),
    ).toBeVisible();

    await siteDetailsPage.dismissNotificationBannerButton.click();
    await expect(siteDetailsPage.notificationBanner).not.toBeVisible();
  },
);

test('A user begins to update the citizen information but cancels', async () => {
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
