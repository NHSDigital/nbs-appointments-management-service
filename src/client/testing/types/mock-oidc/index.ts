type MockOidcUser = {
  subjectId: string;
  username: string;
  password: string;
  claims: [
    {
      // ghcr.io/soluto/oidc-server-mock:latest has a bug and returns a 500 if these properties are not capitalized
      // (but is case insensitive for the other properties above)
      Type: 'email';
      Value: string;
    },
  ];
};

export type { MockOidcUser };
