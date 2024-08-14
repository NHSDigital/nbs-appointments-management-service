import SiteList from '@components/site-list';
import { Site } from '@types';

interface Props {
  sites: Site[];
}

const HomePage = ({ sites }: Props) => {
  return <SiteList sites={sites} />;
};

export default HomePage;
