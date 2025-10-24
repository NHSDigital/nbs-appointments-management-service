import { buildMockOidcUser } from '@e2etests/data';

class MockOidcClient {
  private readonly baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  public async registerTestUser(testId: number) {
    const testUser = buildMockOidcUser(testId);

    await fetch(`${this.baseUrl}/api/v1/user`, {
      method: 'POST',
      body: JSON.stringify(testUser),
      headers: { 'Content-Type': 'application/json' },
    }).then(async response => {
      if (!response.ok) {
        throw new Error(
          `Failed to register a mock OIDC user: ${response.status} ${response.statusText}`,
        );
      }
    });

    // eslint-disable-next-line no-console
    console.log(
      `Registered mock OIDC user: ${testUser.subjectId} with Duende.`,
    );
  }
}

export default MockOidcClient;
