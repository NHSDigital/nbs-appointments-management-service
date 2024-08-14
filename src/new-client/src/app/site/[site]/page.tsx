import NhsPage from '@components/nhs-page';
import { fetchSite } from '@services/appointmentsService';
import { Metadata } from 'next';
import { Site } from '@types';
import NhsNavCard from '@components/nhs-nav-card';

// TODO: Get a brief for what titles/description should be on each page
// Could use the generateMetadata function to dynamically generate this, to include site names / other dynamic content
export const metadata: Metadata = {
  title: 'Appointment Management Service - Site',
  description: 'Manage appointments at this site',
};

type PageProps = {
  params: {
    site: string;
  };
};

const Page = async ({ params }: PageProps) => {
  const site = await fetchSite(params.site);

  return (
    <NhsPage title={site.name} breadcrumbs={[{ name: site.name }]}>
      <SitePage site={site} />
    </NhsPage>
  );
};

interface SitePageProps {
  site: Site;
}

export const SitePage = ({ site }: SitePageProps) => {
  return (
    <>
      <div className="nhsuk-card">
        <div className="nhsuk-card__content">
          <h3 className="nhsuk-card__heading">{site.name}</h3>
          <p className="nhsuk-card__description">{site.address}</p>
        </div>
      </div>
      <ul className="nhsuk-grid-row nhsuk-card-group">
        <li className="nhsuk-grid-column-two-thirds nhsuk-card-group__item">
          <NhsNavCard
            href={`${site.id}/users`}
            title="User Management"
            description="Assign roles to users to give them access to features at this site"
          />
        </li>
      </ul>
    </>
  );
};

export default Page;
