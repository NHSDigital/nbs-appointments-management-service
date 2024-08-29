import { type Locator, type Page } from '@playwright/test';

export default class RootPage {
  readonly page: Page;
  readonly logInButton: Locator;
  readonly logOutButton: Locator;
  readonly serviceName: Locator;

  constructor(page: Page) {
    this.page = page;
    this.serviceName = page.getByRole('link', { name: 'NHS Appointment Book' });
    this.logInButton = page.getByRole('button', { name: 'Log In' });
    this.logOutButton = page.getByRole('button', { name: 'Log Out' });
  }

  async goto() {
    await this.page.goto('/');
  }
}
