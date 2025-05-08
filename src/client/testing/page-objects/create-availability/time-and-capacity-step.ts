/* eslint-disable lines-between-class-members */
import { type Locator, type Page } from '@playwright/test';
import CreateAvailabilityStep from './create-availability-step';
import { Site } from '@types';

export default class TimeAndCapacityStep extends CreateAvailabilityStep {
  readonly title: Locator;

  readonly startTimeHourInput: Locator = this.page.getByRole('textbox', {
    name: 'Session start time - hour',
  });
  readonly startTimeMinuteInput: Locator = this.page.getByRole('textbox', {
    name: 'Session start time - minute',
  });
  readonly endTimeHourInput: Locator = this.page.getByRole('textbox', {
    name: 'Session end time - hour',
  });
  readonly endTimeMinuteInput: Locator = this.page.getByRole('textbox', {
    name: 'Session end time - minute',
  });

  readonly capacityInput: Locator = this.page.getByRole('textbox', {
    name: 'How many vaccinators or vaccination spaces do you have?',
  });

  readonly appointmentLengthInput: Locator = this.page.getByRole('textbox', {
    name: 'How long are your appointments?',
  });

  constructor(page: Page, site: Site) {
    super(page, site);
    this.title = page.getByRole('heading', {
      name: 'What type of session do you want to create?',
    });
  }
}
