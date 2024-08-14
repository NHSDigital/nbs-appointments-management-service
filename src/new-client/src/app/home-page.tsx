import SiteList from '@components/site-list';
import { Site } from '@types';

interface HomePageProps {
  sites: Site[];
}

export const HomePage = ({ sites }: HomePageProps) => {
  return <SiteList sites={sites} />;
};
