import { type Page } from '@playwright/test';
import RootPage from '../root';
import SingleOrRepeatingSessionStep from './single-or-repeating-session-step copy';
import StartAndEndDateStep from './start-and-end-date-step';
import DaysOfWeekStep from './days-of-week-step';
import TimeAndCapacityStep from './time-and-capacity-step';
import SelectServicesStep from './select-services-step';
import SummaryStep from './summary-step';
import { Site } from '@types';

export default class CreateAvailabilityWizardPage extends RootPage {
  readonly site: Site;
  readonly singleOrRepeatingSessionStep: SingleOrRepeatingSessionStep;
  readonly startAndEndDateStep: StartAndEndDateStep;
  readonly daysOfWeekStep: DaysOfWeekStep;
  readonly timeAndCapacityStep: TimeAndCapacityStep;
  readonly selectServicesStep: SelectServicesStep;
  readonly summaryStep: SummaryStep;

  constructor(page: Page, site: Site) {
    super(page);
    this.site = site;

    this.singleOrRepeatingSessionStep = new SingleOrRepeatingSessionStep(page);
    this.startAndEndDateStep = new StartAndEndDateStep(page);
    this.daysOfWeekStep = new DaysOfWeekStep(page);
    this.timeAndCapacityStep = new TimeAndCapacityStep(page);
    this.selectServicesStep = new SelectServicesStep(page);
    this.summaryStep = new SummaryStep(page, site, 'Save session');
  }
}
