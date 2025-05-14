/* eslint-disable lines-between-class-members */
import { type Locator, type Page } from '@playwright/test';
import CreateAvailabilityStep from './create-availability-step';
import CreateAvailabilityPage from './create-availability-page';
import { Site } from '@types';

export default class SummaryStep extends CreateAvailabilityStep {
  readonly repeatingSessionTitle: Locator;
  readonly singleDateSessionTitle: Locator;
  readonly site: Site;

  private getSummaryListItem(label: string): Locator {
    return this.page
      .getByRole('listitem')
      .filter({ has: this.page.getByText(label, { exact: true }) });
  }

  readonly dateSummary: Locator = this.getSummaryListItem('Date');
  readonly datesSummary: Locator = this.getSummaryListItem('Dates');
  readonly daysSummary: Locator = this.getSummaryListItem('Days');
  readonly timeSummary: Locator = this.getSummaryListItem('Time');
  readonly capacitySummary: Locator = this.getSummaryListItem(
    'Vaccinators or vaccination spaces available',
  );
  readonly appointmentLengthSummary: Locator =
    this.getSummaryListItem('Appointment length');
  readonly servicesSummary: Locator =
    this.getSummaryListItem('Services available');

  constructor(page: Page, site: Site, positiveActionButtonText: string) {
    super(page, positiveActionButtonText);
    this.site = site;

    this.repeatingSessionTitle = page.getByRole('heading', {
      name: 'Check weekly session',
    });
    this.singleDateSessionTitle = page.getByRole('heading', {
      name: 'Check single date session',
    });
  }

  async saveSession(): Promise<CreateAvailabilityPage> {
    await this.continueButton.click();
    await this.page.waitForURL(`**/site/${this.site.id}/create-availability`);

    return new CreateAvailabilityPage(this.page, this.site);
  }
}
