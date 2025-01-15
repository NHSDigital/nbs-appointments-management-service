import { type Locator, type Page } from '@playwright/test';
import { expect } from '@playwright/test';
import RootPage from '../root';

export default class EditInformationForCitizensPage extends RootPage {
  readonly title: Locator;
  readonly header: Locator;
  readonly confirmSiteDetailsButton: Locator;
  readonly cancelButton: Locator;
  readonly closeNotificationBannerButton: Locator;
  readonly informationTextField: Locator;
  readonly headerMsg = 'Information for citizens';
  readonly textLimitMsg = 'You have 150 characters remaining';
  readonly testUrl = 'http://localhost:3000/manage-your-appointments/';
  readonly informationWithInvalidChar = 'Test@#$%&*()';
  readonly validationMessage =
    'Site information cannot contain a URL or special characters except full stops, commas, and hyphens';

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', { name: 'Site management' });
    this.header = page.getByRole('heading', {
      name: ' Manage information for citizens',
    });
    this.confirmSiteDetailsButton = page.getByRole('button', {
      name: 'Confirm site details',
    });
    this.cancelButton = page.getByRole('button', { name: 'Cancel' });
    this.closeNotificationBannerButton = page.getByRole('button', {
      name: 'Close',
    });
    this.closeNotificationBannerButton = page.getByRole('button', {
      name: 'Close',
    });
    this.informationTextField = page.getByLabel(
      'What information would you like to include?',
    );
  }

  async setInformationForCitizen(information: string) {
    await this.informationTextField.clear();
    await this.informationTextField.fill(information);
  }

  async save_Cancel_InformationForCitizen(actionType: string) {
    if (actionType == 'Save') {
      await this.confirmSiteDetailsButton.click();
    } else {
      await this.cancelButton.click();
    }
  }

  async verifyInformationForCitizenPageDetails() {
    await expect(this.page.getByText(`${this.headerMsg}`).last()).toBeVisible();
    await expect(this.page.getByText(`${this.textLimitMsg}`)).toBeVisible();
  }

  async VerifyValidationMessage() {
    await this.informationTextField.clear();
    await this.informationTextField.fill(`${this.testUrl}`);
    await this.confirmSiteDetailsButton.click();
    await expect(
      this.page.getByText(`${this.validationMessage}`),
    ).toBeVisible();
    await this.informationTextField.clear();
    await this.informationTextField.fill(`${this.informationWithInvalidChar}`);
    await this.confirmSiteDetailsButton.click();
    await expect(
      this.page.getByText(`${this.validationMessage}`),
    ).toBeVisible();
    await this.cancelButton.click();
  }
}
