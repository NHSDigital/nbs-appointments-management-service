import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';

export default class ChangeAvailabilityPage extends RootPage {
  readonly goBackButton: Locator;
  readonly continueButton: Locator;
  readonly editLengthCapacityRadioOption: Locator;
  readonly cancelRadioOption: Locator;
  readonly confirmCancelRadioOption: Locator;
  readonly changeHeader: Locator;

  constructor(page: Page) {
    super(page);
    this.changeHeader = page.getByRole('heading', { level: 1 }).first();
    this.goBackButton = page.getByRole('link', {
      name: 'Go back',
    });
    this.continueButton = page.getByRole('button', {
      name: 'Continue',
    });
    this.editLengthCapacityRadioOption = page.getByRole('radio', {
      name: 'Change the length or capacity of this session',
    });
    this.cancelRadioOption = page.getByRole('radio', {
      name: 'Cancel this session',
    });
    this.confirmCancelRadioOption = page.getByRole('radio', {
      name: 'Yes, I want to cancel this session',
    });
  }

  async selectChangeType(changeType: 'ChangeLengthCapacity' | 'CancelSession') {
    if (changeType == 'ChangeLengthCapacity') {
      await this.editLengthCapacityRadioOption.click();
    }
    if (changeType == 'CancelSession') {
      await this.cancelRadioOption.click();
    }
  }

  async saveChanges() {
    await this.continueButton.click();
  }
}
