import { SummaryListItem } from '@components/nhsuk-frontend';
import { Site, WellKnownOdsEntry } from '@types';

export const mapSiteOverviewSummaryData = (
  site: Site,
  wellKnownOdsCodeEntries: WellKnownOdsEntry[],
) => {
  if (!site) {
    return undefined;
  }

  const items: SummaryListItem[] = [
    {
      title: 'Address',
      value: site.address.match(/[^,]+,|[^,]+$/g) || [], // Match each word followed by a comma, or the last word without a comma
    },
    { title: 'Phone Number', value: site.phoneNumber },
    { title: 'ODS code', value: site.odsCode },
    {
      title: 'ICB',
      value:
        wellKnownOdsCodeEntries.find(
          e => e.odsCode === site.integratedCareBoard,
        )?.displayName ?? site.integratedCareBoard,
    },
    {
      title: 'Region',
      value:
        wellKnownOdsCodeEntries.find(e => e.odsCode === site.region)
          ?.displayName ?? site.region,
    },
  ];

  return { items, border: false };
};

export const mapCoreSiteSummaryData = (site: Site) => {
  if (!site) {
    return undefined;
  }

  const items: SummaryListItem[] = [
    {
      title: 'Address',
      value: site.address.match(/[^,]+,|[^,]+$/g) || [], // Match each word followed by a comma, or the last word without a comma
    },
  ];

  if (site.location.type === 'Point') {
    items.push({
      title: 'Latitude',
      value: `${site.location.coordinates[0]}`,
    });

    items.push({
      title: 'Longitude',
      value: `${site.location.coordinates[1]}`,
    });

    items.push({ title: 'Phone Number', value: site.phoneNumber });
  }

  return { items, border: false };
};

export const mapAdminSiteSummaryData = (
  site: Site,
  wellKnownOdsCodeEntries: WellKnownOdsEntry[],
) => {
  if (!site) {
    return undefined;
  }

  const items: SummaryListItem[] = [
    { title: 'ODS code', value: site.odsCode },
    {
      title: 'ICB',
      value:
        wellKnownOdsCodeEntries.find(
          e => e.odsCode === site.integratedCareBoard,
        )?.displayName ?? site.integratedCareBoard,
    },
    {
      title: 'Region',
      value:
        wellKnownOdsCodeEntries.find(e => e.odsCode === site.region)
          ?.displayName ?? site.region,
    },
  ];

  return { items, border: false };
};
