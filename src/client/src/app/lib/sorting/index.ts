import { Site } from '@types';
import { Role } from '@types';

const sortSitesByName = (a: Site, b: Site) => {
  if (b.name < a.name) return 1;
  if (b.name > a.name) return -1;
  return 0;
};

const sortRolesByName = (a: Role, b: Role) => {
  if (b.displayName < a.displayName) return 1;
  if (b.displayName > a.displayName) return -1;
  return 0;
};

export { sortSitesByName, sortRolesByName };
