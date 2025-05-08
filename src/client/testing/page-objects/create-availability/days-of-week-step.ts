/* eslint-disable lines-between-class-members */
import { type Locator, type Page } from '@playwright/test';
import CreateAvailabilityStep from './create-availability-step';
import { Site } from '@types';

export default class DaysOfWeekStep extends CreateAvailabilityStep {
  readonly title: Locator;

  readonly mondayCheckbox: Locator = this.page.getByRole('checkbox', {
    name: 'Monday',
  });
  readonly tuesdayCheckbox: Locator = this.page.getByRole('checkbox', {
    name: 'Tuesday',
  });
  readonly wednesdayCheckbox: Locator = this.page.getByRole('checkbox', {
    name: 'Wednesday',
  });
  readonly thursdayCheckbox: Locator = this.page.getByRole('checkbox', {
    name: 'Thursday',
  });
  readonly fridayCheckbox: Locator = this.page.getByRole('checkbox', {
    name: 'Friday',
  });
  readonly saturdayCheckbox: Locator = this.page.getByRole('checkbox', {
    name: 'Saturday',
  });
  readonly sundayCheckbox: Locator = this.page.getByRole('checkbox', {
    name: 'Sunday',
  });

  readonly allDaysCheckbox: Locator = this.page.getByRole('checkbox', {
    name: 'Select all days',
  });

  constructor(page: Page, site: Site) {
    super(page, site);
    this.title = page.getByRole('heading', {
      name: 'Select days to add to your weekly session',
    });
  }
}
