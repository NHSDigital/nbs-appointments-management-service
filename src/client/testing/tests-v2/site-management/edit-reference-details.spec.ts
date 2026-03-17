import { test, expect } from '../../fixtures-v2';

test('A regional user updates the reference details for a site but still has access to it through a site manager role', async ({
  setup,
}) => {
  const { sitePage } = await setup({
    roles: ['canned:site-details-manager', 'system:regional-user'],
  });

  await sitePage
    .clickSiteDetailsCard()
    .then(async siteDetailsPage =>
      siteDetailsPage.clickEditReferenceDetailsLink(),
    )
    .then(async editReferenceDetailsPage => {
      await editReferenceDetailsPage.odsCodeInput.fill('ABC123');
      await editReferenceDetailsPage.icbSelectInput.selectOption(
        'Integrated Care Board 2',
      );
      await editReferenceDetailsPage.regionSelectInput.selectOption('Region 2');

      return editReferenceDetailsPage.saveReferenceDetails();
    })
    .then(async siteDetailsPage => {
      await expect(
        siteDetailsPage.notificationBanner.getByText(
          'You have successfully updated the reference details for the current site.',
        ),
      ).toBeVisible();

      await expect(
        siteDetailsPage.referenceDetailsCard.summaryList.getV10Item('ODS code'),
      ).toHaveText('ABC123');
      await expect(
        siteDetailsPage.referenceDetailsCard.summaryList.getV10Item('ICB'),
      ).toHaveText('Integrated Care Board 2');
      await expect(
        siteDetailsPage.referenceDetailsCard.summaryList.getV10Item('Region'),
      ).toHaveText('Region 2');
    });
});

test('A user updates the reference details for a site but loses access to it as they do', async ({
  setup,
}) => {
  const { site, sitePage } = await setup({
    roles: ['system:regional-user'],
  });

  await sitePage
    .clickSiteDetailsCard()
    .then(async siteDetailsPage =>
      siteDetailsPage.clickEditReferenceDetailsLink(),
    )
    .then(async editReferenceDetailsPage => {
      await editReferenceDetailsPage.odsCodeInput.fill('ABC123');
      await editReferenceDetailsPage.icbSelectInput.selectOption(
        'Integrated Care Board 2',
      );
      await editReferenceDetailsPage.regionSelectInput.selectOption('Region 2');

      return editReferenceDetailsPage.saveReferenceDetailsExpectingLossOfAccess();
    })
    .then(async siteSelectionPage => {
      await expect(siteSelectionPage.title).toBeVisible();
      await expect(
        siteSelectionPage.sitesTable.getByText(site.name),
      ).not.toBeVisible();
    });
});

test('A user starts to update the reference details for a site then changes their mind using the back button', async ({
  setup,
}) => {
  const { site, sitePage } = await setup({
    roles: ['system:regional-user'],
  });
  await sitePage
    .clickSiteDetailsCard()
    .then(siteDetailsPage => siteDetailsPage.clickEditReferenceDetailsLink())
    .then(async editReferenceDetailsPage => {
      await editReferenceDetailsPage.odsCodeInput.fill('Some other name');

      return editReferenceDetailsPage.goBack();
    })
    .then(async siteDetailsPage => {
      await expect(siteDetailsPage.title).toBeVisible();

      await expect(
        siteDetailsPage.referenceDetailsCard.summaryList.getV10Item('ODS code'),
      ).not.toHaveText(/Some other name/);

      await expect(
        siteDetailsPage.referenceDetailsCard.summaryList.getV10Item('ODS code'),
      ).toHaveText(site.odsCode);
    });
});
