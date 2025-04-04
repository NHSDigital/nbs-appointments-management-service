import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';

export default class EditSessionDecisionPage extends RootPage {
  readonly changeHeader: Locator;

  constructor(page: Page) {
    super(page);

    this.changeHeader = page.getByRole('heading', { level: 1 }).first();
  }
}
