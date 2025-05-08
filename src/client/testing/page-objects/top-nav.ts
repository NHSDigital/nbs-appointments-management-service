import { type Locator, type Page } from '@playwright/test';
import { Site } from '@types';
import {
  CreateAvailabilityPage,
  MonthViewAvailabilityPage,
  SiteDetailsPage,
  SiteSelectionPage,
  UsersPage,
} from '@testing-page-objects';

export default class TopNav {
  private readonly page: Page;
  private readonly site: Site;

  private readonly serviceName: Locator;

  private readonly createAvailabilityLink: Locator;
  private readonly viewAvailabilityLink: Locator;
  private readonly manageUsersLink: Locator;
  private readonly manageSiteLink: Locator;

  constructor(page: Page, site: Site) {
    this.page = page;
    this.site = site;

    this.createAvailabilityLink = this.page
      .getByRole('navigation')
      .getByRole('link', { name: 'Create availability' });
    this.viewAvailabilityLink = this.page
      .getByRole('navigation')
      .getByRole('link', { name: 'View availability' });
    this.manageUsersLink = this.page
      .getByRole('navigation')
      .getByRole('link', { name: 'Manage users' });
    this.manageSiteLink = this.page
      .getByRole('navigation')
      .getByRole('link', { name: 'Change site details' });

    this.serviceName = this.page
      .getByRole('banner')
      .getByRole('link', { name: 'Manage Your Appointments' });
  }

  async clickCreateAvailability(): Promise<CreateAvailabilityPage> {
    await this.createAvailabilityLink.click();
    await this.page.waitForURL(`**/site/${this.site.id}/create-availability`);

    return new CreateAvailabilityPage(this.page, this.site);
  }

  async clickViewAvailability(): Promise<MonthViewAvailabilityPage> {
    await this.viewAvailabilityLink.click();
    await this.page.waitForURL(`**/site/${this.site.id}/view-availability`);

    return new MonthViewAvailabilityPage(this.page, this.site);
  }

  async clickManageUsers(): Promise<UsersPage> {
    await this.manageUsersLink.click();
    await this.page.waitForURL(`**/site/${this.site.id}/users`);

    return new UsersPage(this.page, this.site);
  }

  async clickManageSite(): Promise<SiteDetailsPage> {
    await this.manageSiteLink.click();
    await this.page.waitForURL(`**/site/${this.site.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }

  async clickServiceName(): Promise<SiteSelectionPage> {
    await this.serviceName.click();
    await this.page.waitForURL(`**/site/${this.site.id}/details`);

    return new SiteSelectionPage(this.page);
  }
}
