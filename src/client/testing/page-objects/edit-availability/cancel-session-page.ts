import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';

export default class CancelSessionPage extends RootPage {
  readonly cancelSessionHeader: Locator;

  constructor(page: Page) {
    super(page);
    this.cancelSessionHeader = page.getByRole('heading', { level: 1 }).first();
  }
}
