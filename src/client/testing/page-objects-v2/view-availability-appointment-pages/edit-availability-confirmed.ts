import { type Locator, expect } from '@playwright/test';
import { MYALayout } from '@e2etests/types';

export default class EditAvailabilityConfirmedPage extends MYALayout {
  readonly title: Locator = this.page.getByRole('heading', {
    name: /Time and capacity changed for/i,
  });

  async verifySessionUpdated() {
    await expect(this.title).toBeVisible();
  }
}
