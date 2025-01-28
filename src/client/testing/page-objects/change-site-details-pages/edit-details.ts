import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';

export default class EditDetailsPage extends RootPage {
  readonly title: Locator;
  readonly saveAndContinueButton: Locator;
  readonly backLink: Locator;
  readonly closeNotificationBannerButton: Locator;
  readonly nameInput: Locator;
  readonly addressInput: Locator;
  readonly latitudeInput: Locator;
  readonly longitudeInput: Locator;
  readonly phoneNumberInput: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Edit site details',
    });
    this.saveAndContinueButton = page.getByRole('button', {
      name: 'Save and continue',
    });
    this.backLink = page.getByRole('link').filter({ hasText: 'Go back' });
    this.closeNotificationBannerButton = page.getByRole('button', {
      name: 'Close',
    });

    this.nameInput = page.getByRole('textbox', {
      name: 'Site name',
    });

    this.addressInput = page.getByRole('textbox', {
      name: 'Site address',
    });

    this.latitudeInput = page.getByRole('textbox', {
      name: 'Latitude',
    });

    this.longitudeInput = page.getByRole('textbox', {
      name: 'Longitude',
    });

    this.phoneNumberInput = page.getByRole('textbox', {
      name: 'Phone Number',
    });
  }
}
