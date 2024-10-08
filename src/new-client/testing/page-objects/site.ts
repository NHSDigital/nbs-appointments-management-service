import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

export default class SitePage extends RootPage {
  readonly userManagementCard: Locator;
  readonly siteManagementCard: Locator;

  constructor(page: Page) {
    super(page);
    this.userManagementCard = this.page.getByRole('link', {
      name: 'User Management',
    });
    this.siteManagementCard = this.page.getByRole('link', {
      name: 'Site Management',
    });
  }
}
