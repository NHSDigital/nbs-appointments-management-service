import { type Locator, type Page } from '@playwright/test';
import CreateAvailabilityStep from './create-availability-step';
import { Site } from '@types';

export default class StartAndEndDateStep extends CreateAvailabilityStep {
  readonly title: Locator;
  readonly startDateDayInput: Locator;
  readonly startDateMonthInput: Locator;
  readonly startDateYearInput: Locator;

  readonly endDateDayInput: Locator;
  readonly endDateMonthInput: Locator;
  readonly endDateYearInput: Locator;

  constructor(page: Page, site: Site) {
    super(page, site);
    this.title = page.getByRole('heading', {
      name: 'Add start and end dates',
    });

    this.startDateDayInput = page
      .getByRole('group')
      .filter({ has: this.page.getByText('Start date', { exact: true }) })
      .getByRole('textbox', {
        name: 'Day',
      });
    this.startDateMonthInput = page
      .getByRole('group')
      .filter({ has: this.page.getByText('Start date', { exact: true }) })
      .getByRole('textbox', {
        name: 'Month',
      });
    this.startDateYearInput = page
      .getByRole('group')
      .filter({ has: this.page.getByText('Start date', { exact: true }) })
      .getByRole('textbox', {
        name: 'Year',
      });

    this.endDateDayInput = page
      .getByRole('group')
      .filter({ has: this.page.getByText('End date', { exact: true }) })
      .getByRole('textbox', {
        name: 'Day',
      });
    this.endDateMonthInput = page
      .getByRole('group')
      .filter({ has: this.page.getByText('End date', { exact: true }) })
      .getByRole('textbox', {
        name: 'Month',
      });
    this.endDateYearInput = page
      .getByRole('group')
      .filter({ has: this.page.getByText('End date', { exact: true }) })
      .getByRole('textbox', {
        name: 'Year',
      });
  }
}
