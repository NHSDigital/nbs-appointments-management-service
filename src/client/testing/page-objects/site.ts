import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

export default class SitePage extends RootPage {
  readonly userManagementCard: Locator;
  readonly siteManagementCard: Locator;
  readonly createAvailabilityCard: Locator;

  constructor(page: Page) {
    super(page);
    this.userManagementCard = this.page.getByRole('main').getByRole('link', {
      name: 'Manage users',
    });
    this.siteManagementCard = this.page.getByRole('main').getByRole('link', {
      name: 'Change site details and accessibility information',
    });
    this.createAvailabilityCard = this.page
      .getByRole('main')
      .getByRole('link', {
        name: 'Create availability',
      });
  }
}
