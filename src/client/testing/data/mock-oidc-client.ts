/* eslint-disable no-console */
import { MockOidcUser } from '@e2etests/types';

class MockOidcClient {
  private readonly baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  public async registerTestUser(testUser: MockOidcUser) {
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

    console.log(
      `Registered mock OIDC user: ${testUser.subjectId} with Duende.`,
    );
  }
}

export default MockOidcClient;
