import {
  EditInformationForCitizensPage,
  LoginPage,
} from '@testing-page-objects';
import { test, expect } from '../../fixtures';

let put: EditInformationForCitizensPage;

test.beforeEach(async ({ page, getTestSite }) => {
  put = await new LoginPage(page)
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

    await put.infoTextArea.clear();
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

test('A user navigates back to the site details page using the back link', async () => {
  const siteDetailsPage = await put.goBack();

  await expect(siteDetailsPage.title).toBeVisible();
});
