import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';
import { expect } from '@playwright/test';
import { Site } from '@types';
import {
  CreateAvailabilityPage,
  MonthViewAvailabilityPage,
  SiteDetailsPage,
  TopNav,
  UsersPage,
} from '@testing-page-objects';

export default class SitePage extends RootPage {
  readonly site: Site;
  readonly topNav: TopNav;

  readonly userManagementCard: Locator;
  readonly siteManagementCard: Locator;
  readonly createAvailabilityCard: Locator;
  readonly viewAvailabilityAndManageAppointmentsCard: Locator;

  constructor(page: Page, site: Site) {
    super(page);

    this.site = site;
    this.topNav = new TopNav(page, site);

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

  async clickViewAvailabilityCard(): Promise<MonthViewAvailabilityPage> {
    await this.viewAvailabilityAndManageAppointmentsCard.click();
    await this.page.waitForURL(`**/site/${this.site.id}/view-availability`);

    return new MonthViewAvailabilityPage(this.page, this.site);
  }

  async clickSiteDetailsCard(): Promise<SiteDetailsPage> {
    await this.siteManagementCard.click();
    await this.page.waitForURL(`**/site/${this.site.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }

  async clickCreateAvailabilityCard(): Promise<CreateAvailabilityPage> {
    await this.createAvailabilityCard.click();
    await this.page.waitForURL(`**/site/${this.site.id}/create-availability`);

    return new CreateAvailabilityPage(this.page, this.site);
  }

  async clickManageUsersCard(): Promise<UsersPage> {
    await this.userManagementCard.click();
    await this.page.waitForURL(`**/site/${this.site.id}/users`);

    return new UsersPage(this.page, this.site);
  }

  async verifyTileVisible(
    tileName:
      | 'ManageAppointment'
      | 'SiteManagement'
      | 'UserManagement'
      | 'CreateAvailability',
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

  async verifyTileNotVisible(
    tileName: 'UserManagement' | 'CreateAvailability',
  ) {
    if (tileName == 'CreateAvailability') {
      await expect(this.createAvailabilityCard).not.toBeVisible();
    }
    if (tileName == 'UserManagement') {
      await expect(this.userManagementCard).not.toBeVisible();
    }
  }
}
