import { type Page } from '@playwright/test';
import SelectDatesStep from './select-dates-step';
import ConfirmDownloadStep from './confirm-download-step';

export default class ReportsPage {
  readonly selectDatesStep: SelectDatesStep;
  readonly confirmDownloadStep: ConfirmDownloadStep;

  constructor(page: Page) {
    this.selectDatesStep = new SelectDatesStep(page);
    this.confirmDownloadStep = new ConfirmDownloadStep(page);
  }
}
