import { SummaryListItem } from '@components/nhsuk-frontend';
import { Site, WellKnownOdsEntry } from '@types';

export const mapSiteOverviewSummaryData = (
  site: Site,
  wellKnownOdsCodeEntries: WellKnownOdsEntry[],
) => {
  if (!site) {
    return undefined;
  }

  const items: SummaryListItem[] = [];

  items.push({
    title: 'Address',
    value: site.address.match(/[^,]+,|[^,]+$/g) || [], // Match each word followed by a comma, or the last word without a comma
  });
  items.push({ title: 'ODS code', value: site.odsCode });
  items.push({
    title: 'ICB',
    value:
      wellKnownOdsCodeEntries.find(e => e.odsCode === site.integratedCareBoard)
        ?.displayName ?? site.integratedCareBoard,
  });
  items.push({
    title: 'Region',
    value:
      wellKnownOdsCodeEntries.find(e => e.odsCode === site.region)
        ?.displayName ?? site.region,
  });

  const border = false;

  return { items, border };
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
    { title: 'Phone Number', value: site.phoneNumber },
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
