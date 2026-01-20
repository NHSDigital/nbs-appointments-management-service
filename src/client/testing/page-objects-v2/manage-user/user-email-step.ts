import { type Locator } from '@playwright/test';
import ManageUserStep from './manage-user-step';

export default class UserEmailStep extends ManageUserStep {
  title = this.page.getByRole('heading', {
    name: 'Add a user',
  });

  readonly emailInput: Locator = this.page.getByLabel('Enter email address');

  readonly emailHint: Locator = this.page.locator('.nhsuk-hint');
}
