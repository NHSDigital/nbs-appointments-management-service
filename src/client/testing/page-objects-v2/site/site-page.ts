import { type Locator } from '@playwright/test';
import { MYALayout } from '@e2etests/types';

export default class SitePage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: this.site?.name,
  });

  readonly userManagementCard: Locator = this.page
    .getByRole('main')
    .getByRole('link', {
      name: 'Manage users',
    });

  readonly siteManagementCard: Locator = this.page
    .getByRole('main')
    .getByRole('link', {
      name: 'Change site details and accessibility information',
    });

  readonly createAvailabilityCard: Locator = this.page
    .getByRole('main')
    .getByRole('link', {
      name: 'Create availability',
    });

  readonly viewAvailabilityAndManageAppointmentsCard: Locator = this.page
    .getByRole('main')
    .getByRole('link', {
      name: 'View availability and manage appointments for your site',
    });

  //   async clickViewAvailabilityCard(): Promise<MonthViewPage> {
  //     await this.viewAvailabilityAndManageAppointmentsCard.click();
  //     await this.page.waitForURL(`**/site/${this.site.id}/view-availability`);

  //     return new MonthViewPage(this.page, this.site);
  //   }

  //   async clickSiteDetailsCard(): Promise<SiteDetailsPage> {
  //     await this.siteManagementCard.click();
  //     await this.page.waitForURL(`**/site/${this.site.id}/details`);

  //     return new SiteDetailsPage(this.page, this.site);
  //   }

  //   async clickCreateAvailabilityCard(): Promise<CreateAvailabilityPage> {
  //     await this.createAvailabilityCard.click();
  //     await this.page.waitForURL(`**/site/${this.site.id}/create-availability`);

  //     return new CreateAvailabilityPage(this.page, this.site);
  //   }

  //   async clickManageUsersCard(): Promise<UsersPage> {
  //     await this.userManagementCard.click();
  //     await this.page.waitForURL(`**/site/${this.site.id}/users`);

  //     return new UsersPage(this.page, this.site);
  //   }
}
