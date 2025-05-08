import { type Locator, type Page } from '@playwright/test';
import ManageUserStep from './manage-user-step';

export default class NamesStep extends ManageUserStep {
  readonly title: Locator;
  readonly firstNameInput: Locator;
  readonly lastNameInput: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Enter name',
    });
    this.firstNameInput = page.getByLabel('First name');
    this.lastNameInput = page.getByLabel('Last name');
  }
}
