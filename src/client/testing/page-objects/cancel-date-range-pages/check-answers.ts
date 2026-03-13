import RootPage from '../root';
import { expect, Locator, Page } from '../../fixtures';
import { SummaryList } from '@e2etests/types';

export default class CheckAnswersPage extends RootPage {
  readonly title: Locator;
  readonly summaryList: SummaryList;

  constructor(page: Page) {
    super(page);

    this.title = page.getByRole('heading', {
      name: 'Check your answers',
    });

    this.summaryList = new SummaryList(this.page, () => {
      return page.locator('dl');
    });
  }

  async verifySummaryListItemV10ContentValue(title: string, value: string) {
    await expect(this.summaryList.getV10Item(title)).toHaveText(value);
  }
}
