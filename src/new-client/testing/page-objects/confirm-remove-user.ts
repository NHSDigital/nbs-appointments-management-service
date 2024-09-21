import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

export default class ConfirmRemoverUserPage extends RootPage {
  readonly title: Locator;
  readonly confirmRemoveButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Staff Role Management',
    });
    this.confirmRemoveButton = page.getByRole('button', {
      name: 'Yes, remove this account',
    });
    this.cancelButton = page.getByRole('button', {
      name: 'Cancel',
    });
  }
}
