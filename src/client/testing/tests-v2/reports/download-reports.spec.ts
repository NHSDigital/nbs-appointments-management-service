import * as fs from 'fs';
import { test, expect } from '../../fixtures-v2';

test.describe.configure({ mode: 'serial' });

test('Navigates to the reports page via the header before selecting a site - ReportsUplift false', async ({
  setUpSingleSite,
}) => {
  await setUpSingleSite({
    features: [{ name: 'ReportsUplift', enabled: false }],
  })
    .then(async sitePageFixture =>
      sitePageFixture.sitePage.topNav.clickReports(),
    )
    .then(async reportsPage => {
      await expect(reportsPage.selectDatesStep.stepTitle).toBeVisible();
    });
});

test('Navigates to the reports page via a site page - ReportsUplift false', async ({
  setUpSingleSite,
}) => {
  await setUpSingleSite({
    features: [{ name: 'ReportsUplift', enabled: false }],
  })
    .then(async sitePageFixture => sitePageFixture.sitePage.clickReportsCard())
    .then(async reportsPage => {
      await expect(reportsPage.selectDatesStep.stepTitle).toBeVisible();
    });
});

test('Downloads a site summary report - ReportsUplift false', async ({
  page,
  setUpSingleSite,
}) => {
  const fileName = 'downloaded-test-report.csv';

  await setUpSingleSite({
    features: [{ name: 'ReportsUplift', enabled: false }],
  })
    .then(async sitePageFixture =>
      sitePageFixture.sitePage.topNav.clickReports(),
    )
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
      expect(headers).toEqual(expectedSiteSummaryReportHeaders);
    })
    .finally(() => {
      if (fs.existsSync(fileName)) {
        fs.unlinkSync(fileName);
      }
    });
});

test('Navigates to the reports page via the header before selecting a site - ReportsUplift true', async ({
  setUpSingleSite,
}) => {
  await setUpSingleSite({
    features: [{ name: 'ReportsUplift', enabled: true }],
  })
    // Destructured { sitePage } directly for cleaner access to the fixture result
    .then(async ({ sitePage }) => sitePage.topNav.clickReports())
    .then(async reportsPage => {
      await expect(reportsPage.selectReportTypeStep.stepTitle).toBeVisible();
      await expect(
        reportsPage.selectReportTypeStep.siteSummaryReportCard,
      ).toBeVisible();
    });
});

test('Navigates to the reports page via a site page - ReportsUplift true', async ({
  setUpSingleSite,
}) => {
  await setUpSingleSite({
    features: [{ name: 'ReportsUplift', enabled: true }],
  })
    .then(async ({ sitePage }) => sitePage.clickReportsCard())
    .then(async reportsPage => {
      await expect(reportsPage.selectReportTypeStep.stepTitle).toBeVisible();
      await expect(
        reportsPage.selectReportTypeStep.siteSummaryReportCard,
      ).toBeVisible();
    });
});

test('Downloads a site summary report - ReportsUplift true', async ({
  page,
  setUpSingleSite,
}) => {
  // Added Date.now() to ensure unique filenames.
  // This prevents OS-level file-lock errors when running tests in rapid succession.
  const fileName = `site-summary-${Date.now()}.csv`;

  await setUpSingleSite({
    features: [{ name: 'ReportsUplift', enabled: true }],
    roles: ['system:admin-user'],
  })
    .then(async ({ sitePage }) => sitePage.topNav.clickReports())
    .then(async reportsPage => {
      await reportsPage.selectReportTypeStep.siteSummaryReportCard.click();
      await expect(reportsPage.selectReportDatesStep.stepTitle).toBeVisible();

      const today: string = new Date().toISOString().split('T')[0];
      await reportsPage.selectReportDatesStep.startDateInput.fill(today);
      await reportsPage.selectReportDatesStep.endDateInput.fill(today);
      await reportsPage.selectReportDatesStep.continueButton.click();

      await expect(
        reportsPage.confirmReportDownloadStep.stepTitle,
      ).toBeVisible();

      // The "Download Sandwich" (Promise.all).
      // If you await the click first, the event is missed.
      // This ensures the listener is active BEFORE the trigger occurs.
      const [download] = await Promise.all([
        page.waitForEvent('download'),
        reportsPage.confirmReportDownloadStep.continueButton.click(),
      ]);
      return download;
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
      const headers = lines[0].split(',').map(h => h.trim());

      expect(lines.length).toBeGreaterThan(1);
      expect(headers).toEqual(expectedSiteSummaryReportHeaders);
    })
    .finally(() => {
      // Clean up local disk after verification to keep the workspace clean
      if (fs.existsSync(fileName)) {
        fs.unlinkSync(fileName);
      }
    });
});

test('Download all sites report', async ({ page, setUpSingleSite }) => {
  const fileName = `all-sites-${Date.now()}.csv`;

  await setUpSingleSite({
    features: [{ name: 'ReportsUplift', enabled: true }],
    roles: ['system:admin-user'],
  })
    .then(async ({ sitePage }) => sitePage.topNav.clickReports())
    .then(async reportsPage => {
      await reportsPage.selectReportTypeStep.allSitesReportCard.click();
      await expect(
        reportsPage.confirmReportDownloadStep.stepTitle,
      ).toBeVisible();

      const [download] = await Promise.all([
        page.waitForEvent('download'),
        reportsPage.confirmReportDownloadStep.continueButton.click(),
      ]);
      return download;
    })
    .then(async download => {
      expect(download.suggestedFilename()).toContain('MasterSiteListReport');
      await download.saveAs(fileName);

      const csvContent = fs.readFileSync(fileName, 'utf-8');
      const lines = csvContent
        .split('\n')
        .filter(line => line.trim().length > 0);
      const headers = lines[0].split(',').map(h => h.trim());

      expect(lines.length).toBeGreaterThan(1);
      expect(headers).toEqual(expectedAllSitesReportHeaders);
    })
    .finally(() => {
      if (fs.existsSync(fileName)) {
        fs.unlinkSync(fileName);
      }
    });
});

test('Download users report', async ({ page, setUpSingleSite }) => {
  const fileName = `users-${Date.now()}.csv`;

  await setUpSingleSite({
    features: [{ name: 'ReportsUplift', enabled: true }],
    roles: ['system:admin-user'],
  })
    .then(async ({ sitePage }) => sitePage.topNav.clickReports())
    .then(async reportsPage => {
      await reportsPage.selectReportTypeStep.usersReportCard.click();
      await expect(
        reportsPage.confirmReportDownloadStep.stepTitle,
      ).toBeVisible();

      const [download] = await Promise.all([
        page.waitForEvent('download'),
        reportsPage.confirmReportDownloadStep.continueButton.click(),
      ]);
      return download;
    })
    .then(async download => {
      expect(download.suggestedFilename()).toContain('UserReport');
      await download.saveAs(fileName);

      const csvContent = fs.readFileSync(fileName, 'utf-8');
      const lines = csvContent
        .split('\n')
        .filter(line => line.trim().length > 0);

      // Normalize headers by splitting and trimming whitespace/line endings
      const headers = lines[0].split(',').map(h => h.trim());

      expect(lines.length).toBeGreaterThan(1);
      expect(headers).toEqual(expectedUsersReportHeaders);
    })
    .finally(() => {
      if (fs.existsSync(fileName)) {
        fs.unlinkSync(fileName);
      }
    });
});

const expectedSiteSummaryReportHeaders = [
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
];

const expectedUsersReportHeaders = ['User'];

const expectedAllSitesReportHeaders = [
  'Site Name',
  'ODS Code',
  'Site Type',
  'Region',
  'ICB',
  'GUID',
  'IsDeleted',
  'Status',
  'Long',
  'Lat',
  'Address',
];
