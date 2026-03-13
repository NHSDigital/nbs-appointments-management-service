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

  async verifySummaryListItemV10ContentValue(
    title: string,
    value: string,
    action: string | undefined = undefined,
  ) {
    await expect(this.summaryList.getV10Value(title)).toHaveText(value);
    if (action) {
      await expect(this.summaryList.getV10Action(title)).toHaveText(action);
    }
  }
}
