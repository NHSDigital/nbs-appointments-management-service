import NhsHeading from '@components/nhs-heading';
import { fetchSite } from '@services/appointmentsService';
import EditSiteStatusForm from './edit-site-status-form';
import { SummaryList, SummaryListItem } from '@components/nhsuk-frontend';

type Props = {
  siteId: string;
};

export const EditSiteStatusPage = async ({ siteId }: Props) => {
  const siteDetails = await fetchSite(siteId);
  const siteStatus = siteDetails.status;

  const summaryList: SummaryListItem[] = [
    {
      title: 'Current site status',
      value: siteDetails.status?.toString() ?? 'Online',
      tag: {
        colour:
          siteStatus === 'Online' ||
          siteStatus === null ||
          siteStatus === undefined
            ? 'green'
            : 'red',
      },
    },
  ];

  return (
    <>
      <NhsHeading title="Manage site visibility" caption={siteDetails.name} />
      <SummaryList items={summaryList} />
      <EditSiteStatusForm site={siteDetails} />
    </>
  );
};
