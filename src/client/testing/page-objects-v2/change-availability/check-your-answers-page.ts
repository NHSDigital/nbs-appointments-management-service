import { MYALayout } from '@e2etests/types';
import CancellationImpactPage from './cancellation-impact-page';
import SelectDatePage from './select-date-page';
import ConfirmationPage from './confirmation-page';

export default class CheckYourAnswersPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: this.site?.name,
  });

  readonly checkYourAnswersHeading = this.page.getByRole('heading', {
    name: 'Check your answers',
    exact: true,
  });

  readonly cancelSessionsButton = this.page.getByRole('button', {
    name: 'Cancel sessions',
  });

  readonly cancelSessionsAndBookingsButton = this.page.getByRole('button', {
    name: 'Cancel sessions and bookings',
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

  async clickChangeDatesButton(): Promise<SelectDatePage> {
    await this.listItemChangeLink('Dates').click();

    return new SelectDatePage(this.page, this.site);
  }

  async clickCancelSessionsButton(): Promise<ConfirmationPage> {
    await this.cancelSessionsButton.click();

    return new ConfirmationPage(this.page, this.site);
  }

  async clickCancelSessionsAndBookingsButton(): Promise<ConfirmationPage> {
    await this.cancelSessionsAndBookingsButton.click();

    return new ConfirmationPage(this.page, this.site);
  }
}
