import { type Locator } from '@playwright/test';
import { MYALayout } from '@e2etests/types';
import SiteDetailsPage from '../site-details-page';
import SiteSelectionPage from '../../../sites/site-selection-page';

export default class EditReferenceDetailsPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: 'Edit site reference details',
  });

  private readonly backLink: Locator = this.page.getByRole('link', {
    name: 'Go back',
  });

  private readonly saveAndContinueButton: Locator = this.page.getByRole(
    'button',
    {
      name: 'Save and continue',
    },
  );

  readonly odsCodeInput: Locator = this.page.getByRole('textbox', {
    name: 'ODS code',
  });

  readonly icbSelectInput: Locator = this.page.getByRole('combobox', {
    name: 'ICB',
  });

  readonly regionSelectInput: Locator = this.page.getByRole('combobox', {
    name: 'Region',
  });

  async goBack(): Promise<SiteDetailsPage> {
    await this.backLink.click();
    await this.page.waitForURL(`**/site/${this.site?.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }

  async saveReferenceDetails(): Promise<SiteDetailsPage> {
    await this.saveAndContinueButton.click();
    await this.page.waitForURL(`**/site/${this.site?.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }

  async saveReferenceDetailsExpectingLossOfAccess(): Promise<SiteSelectionPage> {
    await this.saveAndContinueButton.click();
    await this.page.waitForURL(`**/sites`);

    return new SiteSelectionPage(this.page, this.site);
  }
}
