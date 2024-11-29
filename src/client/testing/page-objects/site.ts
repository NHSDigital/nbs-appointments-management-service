import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

export default class SitePage extends RootPage {
  readonly userManagementCard: Locator;
  readonly siteManagementCard: Locator;
  readonly createAvailabilityCard: Locator;

  constructor(page: Page) {
    super(page);
    this.userManagementCard = this.page.getByRole('link', {
      name: 'User management',
    });
    this.siteManagementCard = this.page.getByRole('link', {
      name: 'Site management',
    });
    this.createAvailabilityCard = this.page.getByRole('link', {
      name: 'Create availability',
    });
  }
}
