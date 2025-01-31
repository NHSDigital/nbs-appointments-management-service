import { type Locator, type Page } from '@playwright/test';
import env from '../testEnvironment';
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
    user: { Username: string; Password: string } = env.TEST_USERS.testUser1,
  ) {
    await this.page.getByLabel('Username').fill(user.Username);
    await this.page.getByLabel('Password').fill(user.Password);

    await this.page.getByLabel('Password').press('Enter');

    await this.page.waitForURL(env.BASE_URL);
  }
}
