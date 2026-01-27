import { type Locator } from '@playwright/test';
import { MYALayout } from '@e2etests/types';

export default class SiteSummaryReportPage extends MYALayout {
  // Main Page Locators
  readonly title = this.page.getByRole('heading', {
    name: 'Download the report',
  });

  readonly selectDatesStep = {
    stepTitle: this.page.getByRole('heading', {
      name: 'Select the dates and run a report',
    }),
    goBackButton: this.page.getByRole('link', { name: 'Go back' }),
    startDateInput: this.page.getByLabel('Start date'),
    endDateInput: this.page.getByLabel('End date'),
    continueButton: this.page.getByRole('button', { name: 'Create report' }),
  };

  readonly confirmDownloadStep = {
    stepTitle: this.page.getByRole('heading', { name: 'Download the report' }),
    goBackButton: this.page.getByRole('link', { name: 'Go back' }),
    continueButton: this.page.getByRole('button', { name: 'Export data' }),
  };

  async downloadReport() {
    const downloadPromise = this.page.waitForEvent('download');
    await this.confirmDownloadStep.continueButton.click();
    return await downloadPromise;
  }
}
