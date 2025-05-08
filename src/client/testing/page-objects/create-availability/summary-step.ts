/* eslint-disable lines-between-class-members */
import { type Locator, type Page } from '@playwright/test';
import CreateAvailabilityStep from './create-availability-step';
import CreateAvailabilityPage from './create-availability-page';
import { Site } from '@types';

export default class SummaryStep extends CreateAvailabilityStep {
  readonly title: Locator;
  readonly site: Site;

  private getSummaryListItem(label: string): Locator {
    return this.page
      .getByRole('listitem')
      .filter({ has: this.page.getByRole('term', { name: label }) })
      .getByRole('definition');
  }

  readonly datesSummary: Locator = this.getSummaryListItem('Dates').filter({
    hasNot: this.page.getByRole('link', { name: 'Change' }),
  });
  readonly changeDatesLink: Locator = this.getSummaryListItem(
    'Dates',
  ).getByRole('link', { name: 'Change' });

  readonly daysSummary: Locator = this.getSummaryListItem('Days').filter({
    hasNot: this.page.getByRole('link', { name: 'Change' }),
  });
  readonly changeDaysLink: Locator = this.getSummaryListItem('Days').getByRole(
    'link',
    { name: 'Change' },
  );

  readonly timeSummary: Locator = this.getSummaryListItem('Time').filter({
    hasNot: this.page.getByRole('link', { name: 'Change' }),
  });
  readonly changeTimeLink: Locator = this.getSummaryListItem('Time').getByRole(
    'link',
    { name: 'Change' },
  );

  readonly capacitySummary: Locator = this.getSummaryListItem(
    'Vaccinators or vaccination spaces available',
  ).filter({
    hasNot: this.page.getByRole('link', { name: 'Change' }),
  });
  readonly changeCapacityLink: Locator = this.getSummaryListItem(
    'Vaccinators or vaccination spaces available',
  ).getByRole('link', { name: 'Change' });

  readonly appointmentLengthSummary: Locator = this.getSummaryListItem(
    'Appointment length',
  ).filter({
    hasNot: this.page.getByRole('link', { name: 'Change' }),
  });
  readonly changeAppointmentLengthLink: Locator = this.getSummaryListItem(
    'Appointment length',
  ).getByRole('link', { name: 'Change' });

  readonly servicesSummary: Locator = this.getSummaryListItem(
    'Services available',
  ).filter({
    hasNot: this.page.getByRole('link', { name: 'Change' }),
  });
  readonly changeServicesLink: Locator = this.getSummaryListItem(
    'Services available',
  ).getByRole('link', { name: 'Change' });

  constructor(page: Page, site: Site, positiveActionButtonText: string) {
    super(page, positiveActionButtonText);
    this.site = site;

    this.title = page.getByRole('heading', {
      name: 'Check weekly session',
    });
  }

  async saveSession(): Promise<CreateAvailabilityPage> {
    await this.continueButton.click();
    await this.page.waitForURL(`**/site/${this.site.id}/create-availability`);

    return new CreateAvailabilityPage(this.page, this.site);
  }
}
