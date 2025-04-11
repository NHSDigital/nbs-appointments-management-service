import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';

export default class EditSessionDecisionPage extends RootPage {
  readonly changeHeader: Locator;
  readonly editLengthCapacityRadioOption: Locator;
  readonly cancelRadioOption: Locator;
  readonly continueButton: Locator;

  constructor(page: Page) {
    super(page);

    this.changeHeader = page.getByRole('heading', { level: 1 }).first();
    this.editLengthCapacityRadioOption = page.getByRole('radio', {
      name: 'Change the length or capacity of this session',
    });
    this.cancelRadioOption = page.getByRole('radio', {
      name: 'Cancel this session',
    });
    this.continueButton = page.getByRole('button', {
      name: 'Continue',
    });
  }
}
