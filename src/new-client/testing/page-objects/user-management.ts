import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

export default class UserManagementPage extends RootPage {
  readonly title: Locator;
  readonly emailInput: Locator;
  readonly searchUserButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Staff Role Management',
    });
    this.emailInput = page.getByRole('textbox', {
      name: 'Enter an email address',
    });
    this.searchUserButton = page.getByRole('button', {
      name: 'Search user',
    });
    this.cancelButton = page.getByRole('button', {
      name: 'Cancel',
    });
  }
}