import { type Locator } from '@playwright/test';
import { MYALayout } from '@e2etests/types';
import SiteDetailsPage from './details/site-details-page';
import Users from '../manage-user/users';
import { expect } from '@playwright/test';
import SiteSummaryReportPage from './details/site-summary-report-page';

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

  readonly reportsCard: Locator = this.page
    .getByRole('main')
    .getByRole('link', {
      name: 'Report',
    });

  readonly topNav = {
    reportsLink: this.page.getByRole('link', { name: 'Reports', exact: true }),

    clickReports: async (
      reportsUpliftEnabled: boolean,
    ): Promise<SiteSummaryReportPage> => {
      await this.topNav.reportsLink.click();
      reportsUpliftEnabled
        ? await this.page.waitForURL(
            `**/manage-your-appointments/reports/select`,
          )
        : await this.page.waitForURL(`**/manage-your-appointments/reports`);
      return new SiteSummaryReportPage(this.page, this.site);
    },
  };

  readonly buildNumber: Locator = this.page.getByText(/^Build number: /);

  async clickSiteDetailsCard(): Promise<SiteDetailsPage> {
    await this.siteManagementCard.click();
    await this.page.waitForURL(`**/site/${this.site?.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }

  async clickManageUsersCard(): Promise<Users> {
    await this.userManagementCard.click();
    await this.page.waitForURL(`**/site/${this.site?.id}/users`);
    return new Users(this.page, this.site);
  }

  async clickReportsCard(
    reportsUpliftEnabled: boolean,
  ): Promise<SiteSummaryReportPage> {
    await this.reportsCard.click();
    reportsUpliftEnabled
      ? await this.page.waitForURL(`**/manage-your-appointments/reports/select`)
      : await this.page.waitForURL(`**/manage-your-appointments/reports`);
    return new SiteSummaryReportPage(this.page, this.site);
  }

  async clickSiteSummaryReportLink(): Promise<SiteSummaryReportPage> {
    await this.page.getByRole('link', { name: 'Report' }).click();
    await this.page.waitForURL(
      `**/manage-your-appointments/site/${this.site?.id}/report`,
    );
    return new SiteSummaryReportPage(this.page, this.site);
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
