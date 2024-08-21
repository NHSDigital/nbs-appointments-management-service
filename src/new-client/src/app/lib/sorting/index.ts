import { Site } from '@types';

const sortSitesByName = (a: Site, b: Site) => {
  if (b.name < a.name) return 1;
  if (b.name > a.name) return -1;
  return 0;
};

export { sortSitesByName };
