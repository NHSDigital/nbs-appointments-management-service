import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

export default class ServiceNotFoundPage extends RootPage {
  readonly title: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Sorry, there is a problem with this service',
    });
  }
}
