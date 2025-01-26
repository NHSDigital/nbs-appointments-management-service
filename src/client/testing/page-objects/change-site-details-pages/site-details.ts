import { expect } from '../../fixtures';
import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';

export default class SiteDetailsPage extends RootPage {
  readonly title: Locator;
  readonly editSiteAttributesButton: Locator;
  readonly editInformationCitizenButton: Locator;
  readonly closeNotificationBannerButton: Locator;
  readonly headerMsg = 'Manage Site';
  readonly siteDetailsheaderMsg = 'Site Details';
  readonly addressText = 'Address';
  readonly oDSCodeText = 'ODS code';
  readonly iCBText = 'ICB';
  readonly regionText = 'Region';
  readonly accessNeedsheaderMsg = 'Access needs';
  readonly informationForCitizensheaderMsg = 'Information for citizens';

  readonly successBanner =
    "You have successfully updated the current site's information.";

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Site details',
    });
    this.editSiteAttributesButton = page.getByRole('link', {
      name: 'Edit access needs',
    });
    this.editInformationCitizenButton = page.getByRole('link', {
      name: 'Edit information for citizens',
    });

    this.closeNotificationBannerButton = page.getByRole('button', {
      name: 'Close',
    });
  }

  async attributeIsTrue(attributeName: string) {
    await expect(
      this.page
        .getByRole('row')
        .filter({ has: this.page.getByText(attributeName) })
        .getByText('Yes'),
    ).toBeVisible();
  }

  async verifyInformationSaved(information: string) {
    await expect(this.page.getByText(`${this.successBanner}`)).toBeVisible();
    await expect(this.page.getByText(information)).toBeVisible();
    await this.closeNotificationBannerButton.click();
    await expect(this.closeNotificationBannerButton).not.toBeVisible();
  }

  async verifyInformationNotSaved(
    oldInformation: string,
    newInformation: string,
  ) {
    //Will uncomment below line once related defect is fixed.
    //await expect(this.page.getByText(`${this.successBanner}`)).not.toBeVisible();
    await expect(this.page.getByText(oldInformation)).toBeVisible();
    await expect(this.page.getByText(newInformation)).not.toBeVisible();
  }

  async verifySitepage() {
    await expect(
      this.page.getByRole('heading', { name: `${this.headerMsg}` }),
    ).toBeVisible();
    await expect(
      this.page.getByRole('heading', { name: `${this.siteDetailsheaderMsg}` }),
    ).toBeVisible();
    await expect(
      this.page
        .getByRole('listitem')
        .filter({ hasText: `${this.addressText}` }),
    ).toBeVisible();
    await expect(
      this.page
        .getByRole('listitem')
        .filter({ hasText: `${this.oDSCodeText}` }),
    ).toBeVisible();
    await expect(
      this.page.getByRole('listitem').filter({ hasText: `${this.iCBText}` }),
    ).toBeVisible();
    await expect(
      this.page.getByRole('listitem').filter({ hasText: `${this.regionText}` }),
    ).toBeVisible();
    await expect(
      this.page.getByRole('heading', { name: `${this.accessNeedsheaderMsg}` }),
    ).toBeVisible();
    await expect(
      this.page.getByRole('heading', {
        name: `${this.informationForCitizensheaderMsg}`,
      }),
    ).toBeVisible();
  }

  async verifyEditButtonNotVisible() {
    await expect(this.editInformationCitizenButton).not.toBeVisible();
    await expect(this.editSiteAttributesButton).not.toBeVisible();
  }

  async verifyEditButtonToBeVisible() {
    await expect(this.editInformationCitizenButton).toBeVisible();
    await expect(this.editSiteAttributesButton).toBeVisible();
  }
}
