import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

export default class SitePage extends RootPage {
  readonly userManagementCard: Locator;
  readonly siteManagementCard: Locator;
  readonly createAvailabilityCard: Locator;
  readonly viewAvailabilityAndManageAppointmentsCard: Locator;

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
    this.viewAvailabilityAndManageAppointmentsCard = this.page
      .getByRole('main')
      .getByRole('link', {
        name: 'View availability and manage appointments for your site',
      });
  }
}
