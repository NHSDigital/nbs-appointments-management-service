import { MYALayout } from '@e2etests/types';
import { type Locator } from '@playwright/test';

export default class NotFoundPage extends MYALayout {
  readonly title: Locator = this.page.getByRole('heading', {
    name: 'Sorry, we could not find that page',
  });

  readonly notFoundMessageText: Locator = this.page.getByText(
    `You may have typed or pasted the web address incorrectly.`,
  );
}
