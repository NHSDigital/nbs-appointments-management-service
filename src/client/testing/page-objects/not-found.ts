import { type Locator } from '@playwright/test';
import RootPage from './root';

export default class NotFoundPage extends RootPage {
  readonly title: Locator = this.page.getByRole('heading', {
    name: 'Sorry, we could not find that page',
  });

  readonly notFoundMessageText: Locator = this.page.getByText(
    `You may have typed or pasted the web address incorrectly.`,
  );
}
