import * as fs from 'fs';
import { test, expect } from '../../fixtures-v2';

test.describe.configure({ mode: 'serial' });

test('Navigates to the reports page via the header before selecting a site', async ({
  setUpSingleSite,
}) => {
  await setUpSingleSite({
    features: [{ name: 'SiteSummaryReport', enabled: true }],
  })
  .then(async sitePageFixture => sitePageFixture.sitePage.topNav.clickReports())
  .then(async reportsPage => {
    await expect(reportsPage.selectDatesStep.stepTitle).toBeVisible();
  });
});

test('Navigates to the reports page via a site page', async ({
  setUpSingleSite,
}) => {
  await setUpSingleSite({
    features: [{ name: 'SiteSummaryReport', enabled: true }],
  })
  .then(async sitePageFixture => sitePageFixture.sitePage.clickReportsCard())
  .then(async reportsPage => {
    await expect(reportsPage.selectDatesStep.stepTitle).toBeVisible();
  });
});

test('Downloads a site summary report', async ({ page, setUpSingleSite }) => {
  const fileName = 'downloaded-test-report.csv';

  await setUpSingleSite({
    features: [{ name: 'SiteSummaryReport', enabled: true }],
  })
  .then(async sitePageFixture => sitePageFixture.sitePage.topNav.clickReports())
  .then(async reportsPage => {
    const today: string = new Date().toISOString().split('T')[0];
    await reportsPage.selectDatesStep.startDateInput.fill(today);
    await reportsPage.selectDatesStep.endDateInput.fill(today);
    await reportsPage.selectDatesStep.continueButton.click();

    await expect(reportsPage.confirmDownloadStep.stepTitle).toBeVisible();

    const downloadPromise = page.waitForEvent('download');
    await reportsPage.confirmDownloadStep.continueButton.click();
    return downloadPromise;
  })
  .then(async download => {
    expect(download.suggestedFilename()).toContain(
      'GeneralSiteSummaryReport',
    );
    await download.saveAs(fileName);

    const csvContent = fs.readFileSync(fileName, 'utf-8');
    const lines = csvContent
      .split('\n')
      .filter(line => line.trim().length > 0);

    // Normalize headers by splitting and trimming whitespace/line endings
    const headers = lines[0].split(',').map(h => h.trim());

    expect(lines.length).toBeGreaterThan(1);
    expect(headers).toEqual(expectedFileDownloadHeaders);
  })
  .finally(() => {
    if (fs.existsSync(fileName)) {
      fs.unlinkSync(fileName);
    }
  });
});

const expectedFileDownloadHeaders = [
  'Site Name',
  'Status',
  'Site Type',
  'ICB',
  'ICB Name',
  'Region',
  'Region Name',
  'ODS Code',
  'Longitude',
  'Latitude',
  'FLU:2_3 Booked',
  'FLU:18_64 Booked',
  'FLU:65+ Booked',
  'COVID:5_11 Booked',
  'COVID:12_17 Booked',
  'COVID:18+ Booked',
  'COVID_FLU:18_64 Booked',
  'COVID_FLU:65+ Booked',
  'RSV:Adult Booked',
  'COVID_RSV:18+ Booked',
  'Total Bookings',
  'Cancelled',
  'Maximum Capacity',
  'FLU:2_3 Capacity',
  'FLU:18_64 Capacity',
  'FLU:65+ Capacity',
  'COVID:5_11 Capacity',
  'COVID:12_17 Capacity',
  'COVID:18+ Capacity',
  'COVID_FLU:18_64 Capacity',
  'COVID_FLU:65+ Capacity',
  'RSV:Adult Capacity',
  'COVID_RSV:18+ Capacity',
];
