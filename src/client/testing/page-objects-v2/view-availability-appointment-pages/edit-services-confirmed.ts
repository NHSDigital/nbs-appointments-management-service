import { type Locator, expect } from '@playwright/test';
import { MYALayout } from '@e2etests/types';

export interface RemovedServicesOverview {
  date: string;
  sessionTimeInterval: string;
  serviceNames: string;
}

export default class EditServicesConfirmedPage extends MYALayout {
  // Matches "Services removed on [Date]" which is the main <h1>
  readonly title: Locator = this.page.getByRole('heading', {
    name: /Services removed on/i,
  });

  readonly backToWeekViewLink: Locator = this.page.getByRole('link', {
    name: /Back to week view|View availability/i,
  });

  /**
   * Verifies the success message and that the table reflects the removed services.
   */
  async verifyServicesRemoved(removedSessions: RemovedServicesOverview) {
    // Verify Header and Success State
    await expect(this.title).toBeVisible();
    await expect(this.title).toContainText(removedSessions.date);

    // Locate the table
    const sessionTable = this.page.getByRole('table');
    await expect(sessionTable).toBeVisible();

    // Find the specific row by the time interval (e.g., "09:00 - 10:00")
    // This is much safer than using array indexes [1]
    const sessionRow = sessionTable.getByRole('row').filter({
      hasText: removedSessions.sessionTimeInterval,
    });

    await expect(sessionRow).toBeVisible();

    // Verify that the services column in that row contains the expected names
    // We search within the specific row found above
    await expect(sessionRow).toContainText(removedSessions.serviceNames);
  }

  async backToWeekView() {
    await this.backToWeekViewLink.click();
  }
}
