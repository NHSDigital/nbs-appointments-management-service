import { type Locator, type Page } from '@playwright/test';
import ManageUserStep from './manage-user-step';

export default class EmailStep extends ManageUserStep {
  readonly title: Locator;
  readonly emailInput: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Add a user',
    });
    this.emailInput = page.getByLabel('Enter email address');
  }
}
