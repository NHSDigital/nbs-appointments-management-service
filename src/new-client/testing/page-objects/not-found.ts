import { type Locator, type Page } from '@playwright/test';
import RootPage from './root';

export default class NotFoundPage extends RootPage {
  readonly title: Locator;
  readonly warningCalloutHeading: Locator;
  readonly warningCalloutText: Locator;

  constructor(page: Page) {
    super(page);
    this.title = page.getByRole('heading', {
      name: 'Appointment Management Service',
    });
    this.warningCalloutHeading = page.getByRole('heading', {
      name: 'Page not found',
    });
    this.warningCalloutText = page.getByText(
      `The page or resource you're looking for does not exist. Please check the address and try again.`,
    );
  }

  async selectSite(siteName: string) {
    await this.page.getByRole('link', { name: siteName }).click();
  }
}
