import { MYALayout } from '@e2etests/types';
import CancellationImpactPage from './cancellation-impact-page';

export default class CheckYourAnswersPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: this.site?.name,
  });

  readonly checkYourAnswersHeading = this.page.getByRole('heading', {
    name: 'Check your answers',
    exact: true,
  });

  readonly listItemValue = (label: string) =>
    this.page
      .locator('dt')
      .filter({ hasText: label })
      .locator('xpath=following-sibling::dd[1]');

  readonly listItemChangeLink = (label: string) =>
    this.page
      .locator('dt')
      .filter({ hasText: label })
      .locator('xpath=following-sibling::dd[2]')
      .getByRole('link');

  async clickChangeCancellationDecitionButton(
    label: string,
  ): Promise<CancellationImpactPage> {
    await this.listItemChangeLink(label).click();

    return new CancellationImpactPage(this.page, this.site);
  }
}
