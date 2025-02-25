import { type Locator, type Page } from '@playwright/test';
import env from '../testEnvironment';
import { userBySubjectId } from '../fixtures';
import RootPage from './root';

export default class OAuthLoginPage extends RootPage {
  readonly logInButton: Locator;
  readonly logOutButton: Locator;

  constructor(page: Page) {
    super(page);
    this.logInButton = page.getByRole('button', { name: 'Log In' });
    this.logOutButton = page.getByRole('button', { name: 'Log Out' });
  }

  async signIn(
    user: { username: string; password: string } = userBySubjectId(),
  ) {
    await this.page.getByLabel('Username').fill(user.username);
    await this.page.getByLabel('Password').fill(user.password);

    await this.page.getByLabel('Password').press('Enter');

    await this.page.waitForURL(`${env.BASE_URL}/sites`);
  }
}
