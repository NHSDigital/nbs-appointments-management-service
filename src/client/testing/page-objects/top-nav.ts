import { type Locator, type Page } from '@playwright/test';
import { Site } from '@types';
import {
  CreateAvailabilityPage,
  SiteDetailsPage,
  SiteSelectionPage,
  UsersPage,
} from '@testing-page-objects';
import ReportsPage from './reports/reports';

export default class TopNav {
  private readonly page: Page;
  private readonly site?: Site;

  private readonly serviceName: Locator;

  private readonly createAvailabilityLink: Locator;
  private readonly manageUsersLink: Locator;
  private readonly manageSiteLink: Locator;
  private readonly reportsLink: Locator;

  constructor(page: Page, site?: Site) {
    this.page = page;
    this.site = site;

    this.createAvailabilityLink = this.page
      .getByRole('navigation')
      .getByRole('link', { name: 'Create availability' });
    this.manageUsersLink = this.page
      .getByRole('navigation')
      .getByRole('link', { name: 'Manage users' });
    this.manageSiteLink = this.page
      .getByRole('navigation')
      .getByRole('link', { name: 'Change site details' });
    this.reportsLink = this.page
      .getByRole('navigation')
      .getByRole('link', { name: 'Reports' });

    this.serviceName = this.page
      .getByRole('banner')
      .filter({ hasText: 'Manage Your Appointments' })
      .getByRole('link', { name: 'Manage Your Appointments' });
  }

  async clickCreateAvailability(): Promise<CreateAvailabilityPage> {
    if (!this.site) {
      throw new Error('Page object not initialized with a site');
    }

    await this.createAvailabilityLink.click();
    await this.page.waitForURL(`**/site/${this.site.id}/create-availability`);

    return new CreateAvailabilityPage(this.page);
  }

  async clickManageUsers(): Promise<UsersPage> {
    if (!this.site) {
      throw new Error('Page object not initialized with a site');
    }

    await this.manageUsersLink.click();
    await this.page.waitForURL(`**/site/${this.site.id}/users`);

    return new UsersPage(this.page);
  }

  async clickManageSite(): Promise<SiteDetailsPage> {
    if (!this.site) {
      throw new Error('Page object not initialized with a site');
    }

    await this.manageSiteLink.click();
    await this.page.waitForURL(`**/site/${this.site.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }

  async clickServiceName(): Promise<SiteSelectionPage> {
    if (!this.site) {
      throw new Error('Page object not initialized with a site');
    }

    await this.serviceName.click();
    await this.page.waitForURL(`**/site/${this.site.id}/details`);

    return new SiteSelectionPage(this.page);
  }

  async clickReports(): Promise<ReportsPage> {
    await this.reportsLink.click();
    await this.page.waitForURL(`**/reports`);

    return new ReportsPage(this.page);
  }
}
