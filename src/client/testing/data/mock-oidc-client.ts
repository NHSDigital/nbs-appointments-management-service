import { MockOidcUser } from '@e2etests/types';

class MockOidcClient {
  private readonly baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  private buildTestUser(testId: number): MockOidcUser {
    return {
      subjectId: `test-user-${testId}@nhs.net`,
      username: `Test User ${testId}`,
      password: `TestUserPassword123!`,
      claims: [{ type: 'email', value: `test-user-${testId}@nhs.net` }],
    };
  }

  public async registerTestUser(testId: number) {
    const testUser = this.buildTestUser(testId);

    await fetch(`${this.baseUrl}/api/v1/user`, {
      method: 'POST',
      body: JSON.stringify(testUser),
    }).then(async response => {
      if (!response.ok) {
        throw new Error(
          `Failed to register a mock OIDC user: ${response.status} ${response.statusText}`,
        );
      }
    });
  }
}

export default MockOidcClient;
