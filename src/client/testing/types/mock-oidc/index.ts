type MockOidcUser = {
  subjectId: string;
  username: string;
  password: string;
  claims: [
    {
      type: 'email';
      value: string;
    },
  ];
};

export type { MockOidcUser };
