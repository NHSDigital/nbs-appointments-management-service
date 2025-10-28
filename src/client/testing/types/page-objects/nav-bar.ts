import { type Locator, type Page } from '@playwright/test';
import PageObject from './page-object';
import { E2ETestSite } from '..';

export default class NavBar extends PageObject {
  private readonly site?: E2ETestSite;

  constructor(page: Page, site?: E2ETestSite) {
    super(page);
    this.site = site;
  }

  private readonly serviceName: Locator = this.page
    .getByRole('banner')
    .filter({ hasText: 'Manage Your Appointments' })
    .getByRole('link', { name: 'Manage Your Appointments' });

  private readonly createAvailabilityLink: Locator = this.page
    .getByRole('navigation')
    .getByRole('link', { name: 'Create availability' });

  private readonly viewAvailabilityLink: Locator = this.page
    .getByRole('navigation')
    .getByRole('link', { name: 'View availability' });

  private readonly manageUsersLink: Locator = this.page
    .getByRole('navigation')
    .getByRole('link', { name: 'Manage users' });

  private readonly manageSiteLink: Locator = this.page
    .getByRole('navigation')
    .getByRole('link', { name: 'Change site details' });

  private readonly reportsLink: Locator = this.page
    .getByRole('navigation')
    .getByRole('link', { name: 'Reports' });

  // async clickCreateAvailability(): Promise<CreateAvailabilityPage> {
  //   if (!this.site) {
  //     throw new Error(
  //       'The NavBar page object requires a site to perform this action.',
  //     );
  //   }

  //   await this.createAvailabilityLink.click();
  //   await this.page.waitForURL(`**/site/${this.site.id}/create-availability`);

  //   return new CreateAvailabilityPage(this.page, this.site);
  // }

  // async clickViewAvailability(): Promise<MonthViewPage> {
  //   await this.viewAvailabilityLink.click();
  //   await this.page.waitForURL(`**/site/${this.site.id}/view-availability`);

  //   return new MonthViewPage(this.page, this.site);
  // }

  // async clickManageUsers(): Promise<UsersPage> {
  //   await this.manageUsersLink.click();
  //   await this.page.waitForURL(`**/site/${this.site.id}/users`);

  //   return new UsersPage(this.page, this.site);
  // }

  // async clickManageSite(): Promise<SiteDetailsPage> {
  //   await this.manageSiteLink.click();
  //   await this.page.waitForURL(`**/site/${this.site.id}/details`);

  //   return new SiteDetailsPage(this.page, this.site);
  // }

  // async clickServiceName(): Promise<SiteSelectionPage> {
  //   await this.serviceName.click();
  //   await this.page.waitForURL(`**/site/${this.site.id}/details`);

  //   return new SiteSelectionPage(this.page);
  // }
}
