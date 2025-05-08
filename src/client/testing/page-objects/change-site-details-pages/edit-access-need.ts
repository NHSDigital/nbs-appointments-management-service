import { type Locator, type Page } from '@playwright/test';
import RootPage from '../root';
import { Site } from '@types';
import { SiteDetailsPage } from '@testing-page-objects';

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

export default class EditAccessNeedsPage extends RootPage {
  readonly site: Site;
  readonly title: Locator;

  readonly confirmSiteDetailsButton: Locator;
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

  constructor(page: Page, site: Site) {
    super(page);
    this.site = site;
    this.title = page.getByRole('heading', {
      name: 'Site management',
    });
    this.confirmSiteDetailsButton = page.getByRole('button', {
      name: 'Confirm site details',
    });
  }

  async saveSiteDetails(): Promise<SiteDetailsPage> {
    await this.confirmSiteDetailsButton.click();
    await this.page.waitForURL(`**/site/${this.site.id}/details`);

    return new SiteDetailsPage(this.page, this.site);
  }
}
