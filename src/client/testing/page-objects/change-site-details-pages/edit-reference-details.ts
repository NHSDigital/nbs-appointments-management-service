import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';

export default class EditReferenceDetailsPage extends RootPage {
  readonly title: Locator;
  readonly saveAndContinueButton: Locator;
  readonly backLink: Locator;
  readonly closeNotificationBannerButton: Locator;

  readonly odsCodeInput: Locator;
  readonly icbSelectInput: Locator;
  readonly regionSelectInput: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Edit site reference details',
    });
    this.saveAndContinueButton = page.getByRole('button', {
      name: 'Save and continue',
    });
    this.backLink = page.getByRole('link').filter({ hasText: 'Go back' });
    this.closeNotificationBannerButton = page.getByRole('button', {
      name: 'Close',
    });

    this.odsCodeInput = page.getByRole('textbox', {
      name: 'ODS code',
    });

    this.icbSelectInput = page.getByRole('combobox', {
      name: 'ICB',
    });

    this.regionSelectInput = page.getByRole('combobox', {
      name: 'Region',
    });
  }
}
