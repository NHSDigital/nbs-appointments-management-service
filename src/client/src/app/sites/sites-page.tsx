import SiteList from '@components/site-list';
import { Site } from '@types';

type SitesPageProps = {
  sites: Site[];
};

export const SitesPage = ({ sites }: SitesPageProps) => {
  // TODO: Replace this message with approved copy
  if (sites.length === 0) {
    return <p>You have not been assigned to any sites.</p>;
  }

  return <SiteList sites={sites} />;
};
