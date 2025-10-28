import { CosmosClient } from '@azure/cosmos';
import { Role } from '@e2etests/types';
import { buildSiteDocument, buildUserDocument } from '@e2etests/data';

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

  public async createSite(testId: number) {
    const siteDocument = buildSiteDocument(testId);

    const database = await this.getDatabase();
    const { container } = await database.containers.createIfNotExists({
      id: this.coreContainerId,
    });
    await container.items.upsert(siteDocument);

    // eslint-disable-next-line no-console
    console.log(`Written site: ${siteDocument.id} to Cosmos DB.`);
  }

  public async createUser(testId: number, roles: Role[]) {
    const userDocument = buildUserDocument(testId, roles);

    const database = await this.getDatabase();
    const { container } = await database.containers.createIfNotExists({
      id: this.coreContainerId,
    });
    await container.items.upsert(userDocument);

    // eslint-disable-next-line no-console
    console.log(`Written user: ${userDocument.id} to Cosmos DB.`);
  }
}

export default CosmosDbClient;
