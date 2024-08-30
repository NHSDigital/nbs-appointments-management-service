import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

export default class SitePage extends RootPage {
  readonly userManagementCard: Locator;

  constructor(page: Page) {
    super(page);
    this.userManagementCard = this.page.getByRole('link', {
      name: 'User Management',
    });
  }
}
