/* eslint-disable lines-between-class-members */
import { MYALayout } from '@e2etests/types';
import { type Locator } from '@playwright/test';
import SiteDetailsPage from '../site-details-page';

type accessNeedsCheckboxes = {
  accessibleToilet: Locator;
  brailleTranslationService: Locator;
  disabledCarParking: Locator;
  carParking: Locator;
  inductionLoop: Locator;
  signLanguageService: Locator;
  stepFreeAccess: Locator;
  textRelay: Locator;
  wheelchairAccess: Locator;
};

export default class EditAccessNeedsPage extends MYALayout {
  title = this.page.getByRole('heading', {
    name: 'Edit accessibilities',
  });

  private readonly cancelButton: Locator = this.page.getByRole('button', {
    name: 'Cancel',
  });

  private readonly saveAndContinueButton: Locator = this.page.getByRole(
    'button',
    {
      name: 'Confirm site details',
    },
  );

  readonly checkboxes: accessNeedsCheckboxes = {
    accessibleToilet: this.page.getByRole('checkbox', {
      name: 'Accessible toilet',
    }),
    brailleTranslationService: this.page.getByRole('checkbox', {
      name: 'Braille translation service',
    }),
    disabledCarParking: this.page.getByRole('checkbox', {
      name: 'Disabled car parking',
    }),
    carParking: this.page.getByRole('checkbox', {
      name: 'Car parking',
    }),
    inductionLoop: this.page.getByRole('checkbox', {
      name: 'Induction loop',
    }),
    signLanguageService: this.page.getByRole('checkbox', {
      name: 'Sign language service',
    }),
    stepFreeAccess: this.page.getByRole('checkbox', {
      name: 'Step free access',
    }),
    textRelay: this.page.getByRole('checkbox', {
      name: 'Text relay',
    }),
    wheelchairAccess: this.page.getByRole('checkbox', {
      name: 'Wheelchair access',
    }),
  };

  async cancel(): Promise<SiteDetailsPage> {
    await this.cancelButton.click();
    await this.page.waitForURL(`**/site/${this.site?.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }

  async saveSiteDetails(): Promise<SiteDetailsPage> {
    await this.saveAndContinueButton.click();
    await this.page.waitForURL(`**/site/${this.site?.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }
}
