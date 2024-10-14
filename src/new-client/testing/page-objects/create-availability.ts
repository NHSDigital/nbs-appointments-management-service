import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

export default class CreateAvailabilityPage extends RootPage {
  readonly title: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Create Availability',
    });
  }
}
