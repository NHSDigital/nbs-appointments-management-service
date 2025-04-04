import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';

export default class EditSessionPage extends RootPage {
  readonly editSessionHeader: Locator;
  readonly capacityInput: Locator;

  readonly startHourInput: Locator;
  readonly startMinuteInput: Locator;

  readonly endHourInput: Locator;
  readonly endMinuteInput: Locator;

  constructor(page: Page) {
    super(page);

    this.editSessionHeader = page.getByRole('heading', { level: 1 }).first();
    this.startHourInput = page.getByRole('textbox', {
      name: 'Session start time - hour',
    });
    this.startMinuteInput = page.getByRole('textbox', {
      name: 'Session start time - minute',
    });
    this.endHourInput = page.getByRole('textbox', {
      name: 'Session end time - hour',
    });
    this.endMinuteInput = page.getByRole('textbox', {
      name: 'Session end time - minute',
    });
    this.capacityInput = page.getByRole('textbox', {
      name: 'How many vaccinators or vaccination spaces do you have?',
    });
  }
}
