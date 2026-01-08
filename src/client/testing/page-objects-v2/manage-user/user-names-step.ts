import { type Locator } from '@playwright/test';
import ManageUserStep from './manage-user-step';

export default class UserNamesStep extends ManageUserStep {
  title = this.page.getByRole('heading', {
    name: 'Enter name',
  });

  readonly firstNameInput: Locator = this.page.getByLabel('First name');

  readonly lastNameInput: Locator = this.page.getByLabel('Last name');
}
