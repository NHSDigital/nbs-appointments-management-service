import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

export default class NotFoundPage extends RootPage {
  readonly title: Locator;
  readonly notFoundMessageText: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Sorry, we could not find that page',
    });
    this.notFoundMessageText = page.getByText(
      `You may have typed or pasted the web address incorrectly.`,
    );
  }
}
