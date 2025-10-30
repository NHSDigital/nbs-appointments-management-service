import { expect } from '@playwright/test';
import RootPage from '../root';

export default class EditAvailabilityConfirmedPage extends RootPage {
  async verifySessionUpdated() {
    await expect(
      this.page.getByText('Time and capacity changed for'),
    ).toBeVisible();
  }
}
