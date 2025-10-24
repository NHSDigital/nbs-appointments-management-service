type FeatureFlag = {
  name: string;
  enabled: boolean;
};

type E2ETestUser = {
  id: string;
  username: string;
  password: string;
};

type E2ETestSite = {
  id: string;
  name: string;
};

export type { FeatureFlag, E2ETestUser, E2ETestSite };
export * from './mock-oidc';
export * from './page-objects';
export * from './cosmos';
