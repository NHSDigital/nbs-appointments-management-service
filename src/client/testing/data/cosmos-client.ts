import { CosmosClient } from '@azure/cosmos';
import { Role, SiteDocument, UserDocument } from '@e2etests/types';

class CosmosDbClient {
  private readonly client: CosmosClient;

  private readonly coreContainerId = 'core_data';
  private readonly bookingContainerId = 'booking_data';
  private readonly indexContainerId = 'index_data';

  constructor(cosmosEndpoint: string, cosmosToken: string) {
    this.client = new CosmosClient({
      endpoint: cosmosEndpoint,
      key: cosmosToken,
    });
  }

  private async getDatabase() {
    const { database: appts } = await this.client.databases.createIfNotExists({
      id: 'appts',
    });

    return appts;
  }

  private buildSiteDocument(testId: number): SiteDocument {
    return {
      id: this.buildSiteId(testId),
      docType: 'site',
      name: `Test Site ${testId}`,
      address: '123 Test St, Testville, TE5 7ST',
      phoneNumber: '0113 1111111',
      odsCode: this.buildOdsCode(testId),
      region: this.buildRegion(testId),
      integratedCareBoard: this.buildIcb(testId),
      location: {
        type: 'Point',
        coordinates: [-1.6610648, 53.795467],
      },
      accessibilities: [],
      informationForCitizens:
        'This is some placeholder information for citizens.',
    };
  }

  private buildSiteId(testId: number): string {
    return `site-${testId}`;
  }

  private buildOdsCode(testId: number): string {
    return `ABC${testId}`;
  }

  private buildRegion(testId: number): string {
    return `R${testId}`;
  }

  private buildIcb(testId: number): string {
    return `ICB${testId}`;
  }

  private buildScopeForRole(testId: number, role: Role): string {
    switch (role) {
      case 'system:admin-user':
        return `global`;
      case 'system:icb-user':
        return this.buildIcb(testId);
      case 'system:regional-user':
        return this.buildRegion(testId);
      default:
        return this.buildSiteId(testId);
    }
  }

  private buildUserDocument(testId: number, role: Role): UserDocument {
    return {
      id: `test-user-${testId}@nhs.net`,
      docType: 'user',
      latestAcceptedEulaVersion: '2024-12-01',
      roleAssignments: [{ role, scope: this.buildScopeForRole(testId, role) }],
    };
  }

  public async createSite(testId: number) {
    const siteDocument = this.buildSiteDocument(testId);

    const database = await this.getDatabase();
    const { container } = await database.containers.createIfNotExists({
      id: this.coreContainerId,
    });
    await container.items.upsert(siteDocument);
  }

  public async createUser(testId: number, role: Role) {
    const userDocument = this.buildUserDocument(testId, role);

    const database = await this.getDatabase();
    const { container } = await database.containers.createIfNotExists({
      id: this.coreContainerId,
    });
    await container.items.upsert(userDocument);
  }
}

export default CosmosDbClient;
