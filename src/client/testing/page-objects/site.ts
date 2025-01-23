import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';
import { expect } from '@playwright/test';

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

  async veriyTileVisible(
    tileName: 'ManageAppointment' | 'SiteManagement' | 'CreateAvailability',
  ) {
    if (tileName == 'ManageAppointment') {
      await expect(
        this.viewAvailabilityAndManageAppointmentsCard,
      ).toBeVisible();
    }
    if (tileName == 'SiteManagement') {
      await expect(this.siteManagementCard).toBeVisible();
    }
    if (tileName == 'CreateAvailability') {
      await expect(this.createAvailabilityCard).toBeVisible();
    }
  }

  async veriyTileNotVisible(tileName: 'UserManagement' | 'CreateAvailability') {
    if (tileName == 'CreateAvailability') {
      await expect(this.createAvailabilityCard).not.toBeVisible();
    }
    if (tileName == 'UserManagement') {
      await expect(this.userManagementCard).not.toBeVisible();
    }
  }

  //   async veriyTileNotViisible(){
  //     // await expect(
  //     //    sitePage.viewAvailabilityAndManageAppointmentsCard,
  //     //  ).toBeVisible();
  //     //  await expect(sitePage.createAvailabilityCard).not.toBeVisible();
  //     //  await expect(sitePage.userManagementCard).not.toBeVisible();
  //     //  await expect(sitePage.siteManagementCard).toBeVisible();
  //  };
}
