/* eslint-disable lines-between-class-members */
import { type Locator } from '@playwright/test';
import { MYALayout, SummaryList } from '@e2etests/types';
import EditReferenceDetailsPage from './edit-reference-details/edit-reference-details-page';
import EditDetailsPage from './edit-details/edit-details-page';
import EditInformationForCitizensPage from './edit-information-for-citizens/edit-information-for-citizens-page';
import EditAccessNeedsPage from './edit-accessibilities/edit-access-needs-page';
import EditSiteStatusPage from './edit-site-status/edit-site-status-page';

type SiteDetailsCard = {
  title: Locator;
  summaryList: SummaryList;
  editLinks: Locator[];
};

export default class SiteDetailsPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: 'Manage Site',
  });

  private card(sectionTitle: string): Locator {
    return this.page.locator('.nhsuk-card').filter({
      has: this.page.getByRole('heading', { name: sectionTitle }),
    });
  }

  public detailsCard: SiteDetailsCard = {
    title: this.card('Site details').getByRole('heading', {
      name: 'Site details',
    }),
    summaryList: new SummaryList(this.page, () => {
      return this.card('Site details').locator('dl');
    }),
    editLinks: [
      this.card('Site details').getByRole('link', {
        name: 'Edit site details',
      }),
      this.card('Site details').getByRole('link', {
        name: 'Change site status',
      }),
    ],
  };

  public referenceDetailsCard: SiteDetailsCard = {
    title: this.card('Site reference details').getByRole('heading', {
      name: 'Site reference details',
    }),
    summaryList: new SummaryList(this.page, () => {
      return this.card('Site reference details').locator('dl');
    }),
    editLinks: [
      this.card('Site reference details').getByRole('link', {
        name: 'Edit site reference details',
      }),
    ],
  };

  public accessNeedsCard: SiteDetailsCard = {
    title: this.card('Access needs').getByRole('heading', {
      name: 'Access needs',
    }),
    summaryList: new SummaryList(this.page, () => {
      return this.card('Access needs').locator('dl');
    }),
    editLinks: [
      this.card('Access needs').getByRole('link', {
        name: 'Edit access needs',
      }),
    ],
  };

  public informationForCitizensCard = {
    title: this.card('Information for citizens').getByRole('heading', {
      name: 'Information for citizens',
    }),
    editLinks: [
      this.card('Information for citizens').getByRole('link', {
        name: 'Edit information for citizens',
      }),
    ],
    content: this.card('Information for citizens'),
  };

  async clickEditDetailsLink(): Promise<EditDetailsPage> {
    await this.detailsCard.editLinks[0].click();
    await this.page.waitForURL(
      `**/manage-your-appointments/site/${this.site?.id}/details/edit-details`,
    );

    return new EditDetailsPage(this.page, this.site);
  }

  async clickEditSiteStatusLink(): Promise<EditSiteStatusPage> {
    await this.detailsCard.editLinks[1].click();
    await this.page.waitForURL(
      `**/manage-your-appointments/site/${this.site?.id}/details/edit-site-status`,
    );

    return new EditSiteStatusPage(this.page, this.site);
  }

  async clickEditReferenceDetailsLink(): Promise<EditReferenceDetailsPage> {
    await this.referenceDetailsCard.editLinks[0].click();
    await this.page.waitForURL(
      `**/manage-your-appointments/site/${this.site?.id}/details/edit-reference-details`,
    );

    return new EditReferenceDetailsPage(this.page, this.site);
  }

  async clickEditAccessNeedsLink(): Promise<EditAccessNeedsPage> {
    await this.accessNeedsCard.editLinks[0].click();
    await this.page.waitForURL(
      `**/manage-your-appointments/site/${this.site?.id}/details/edit-accessibilities`,
    );

    return new EditAccessNeedsPage(this.page, this.site);
  }

  async clickEditInformationForCitizensLink(): Promise<EditInformationForCitizensPage> {
    await this.informationForCitizensCard.editLinks[0].click();
    await this.page.waitForURL(
      `**/manage-your-appointments/site/${this.site?.id}/details/edit-information-for-citizens`,
    );

    return new EditInformationForCitizensPage(this.page, this.site);
  }
}
