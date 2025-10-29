/* eslint-disable lines-between-class-members */
import { type Locator, type Page } from '@playwright/test';
import { MYALayout, SummaryList } from '@e2etests/types';
import EditReferenceDetailsPage from './edit-reference-details/edit-reference-details-page';
import EditDetailsPage from './edit-details/edit-details-page';
import EditInformationForCitizensPage from './edit-information-for-citizens/edit-information-for-citizens-page';
import EditAccessNeedsPage from './edit-accessibilities/edit-access-needs-page';

type SiteDetailsCard = {
  title: Locator;
  summaryList: SummaryList;
  editLink: Locator;
};

export default class SiteDetailsPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: 'Manage Site',
  });

  public detailsCard: SiteDetailsCard = {
    title: this.page
      .getByRole('listitem')
      .filter({
        has: this.page.getByRole('heading', { name: 'Site details' }),
      })
      .getByRole('heading', {
        name: 'Site details',
      }),
    summaryList: new SummaryList(this.page, (page: Page) => {
      return page.getByRole('listitem').filter({
        has: this.page.getByRole('heading', { name: 'Site details' }),
      });
    }),
    editLink: this.page.getByRole('link', {
      name: 'Edit site details',
    }),
  };

  public referenceDetailsCard: SiteDetailsCard = {
    title: this.page
      .getByRole('listitem')
      .filter({
        has: this.page.getByRole('heading', { name: 'Site reference details' }),
      })
      .getByRole('heading', {
        name: 'Site reference details',
      }),
    summaryList: new SummaryList(this.page, (page: Page) => {
      return page.getByRole('listitem').filter({
        has: this.page.getByRole('heading', { name: 'Site reference details' }),
      });
    }),
    editLink: this.page.getByRole('link', {
      name: 'Edit site reference details',
    }),
  };

  public accessNeedsCard: SiteDetailsCard = {
    title: this.page
      .getByRole('listitem')
      .filter({
        has: this.page.getByRole('heading', { name: 'Access needs' }),
      })
      .getByRole('heading', {
        name: 'Access needs',
      }),
    summaryList: new SummaryList(this.page, (page: Page) => {
      return page.getByRole('listitem').filter({
        has: this.page.getByRole('heading', { name: 'Access needs' }),
      });
    }),
    editLink: this.page.getByRole('link', {
      name: 'Edit access needs',
    }),
  };

  public informationForCitizensCard = {
    title: this.page
      .getByRole('listitem')
      .filter({
        has: this.page.getByRole('heading', {
          name: 'Information for citizens',
        }),
      })
      .getByRole('heading', {
        name: 'Information for citizens',
      }),
    editLink: this.page.getByRole('link', {
      name: 'Edit information for citizens',
    }),
  };

  async clickEditDetailsLink(): Promise<EditDetailsPage> {
    await this.detailsCard.editLink.click();
    await this.page.waitForURL(`**/site/${this.site?.id}/details/edit-details`);

    return new EditDetailsPage(this.page, this.site);
  }

  async clickEditReferenceDetailsLink(): Promise<EditReferenceDetailsPage> {
    await this.referenceDetailsCard.editLink.click();
    await this.page.waitForURL(
      `**/site/${this.site?.id}/details/edit-reference-details`,
    );

    return new EditReferenceDetailsPage(this.page, this.site);
  }

  async clickEditAccessNeedsLink(): Promise<EditAccessNeedsPage> {
    await this.accessNeedsCard.editLink.click();
    await this.page.waitForURL(
      `**/site/${this.site?.id}/details/edit-accessibilities`,
    );

    return new EditAccessNeedsPage(this.page, this.site);
  }

  async clickEditInformationForCitizensLink(): Promise<EditInformationForCitizensPage> {
    await this.informationForCitizensCard.editLink.click();
    await this.page.waitForURL(
      `**/site/${this.site?.id}/details/edit-information-for-citizens`,
    );

    return new EditInformationForCitizensPage(this.page, this.site);
  }
}
