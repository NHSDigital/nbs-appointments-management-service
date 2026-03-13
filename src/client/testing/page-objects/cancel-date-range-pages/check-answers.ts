import RootPage from '../root';
import { expect, Locator, Page } from '../../fixtures';

export default class CheckAnswersPage extends RootPage {
  readonly title: Locator;

  constructor(page: Page) {
    super(page);

    this.title = page.getByRole('heading', {
      name: 'Check your answers',
    });
  }

  async verifySummaryListItemV10ContentValue(title: string, value: string) {
    await expect(this.summaryList.getV10Item(title)).toHaveText(value);
  }
}
