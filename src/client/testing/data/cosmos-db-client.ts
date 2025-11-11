/* eslint-disable no-console */
import { CosmosClient } from '@azure/cosmos';
import { Role, SiteDocument } from '@e2etests/types';
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

  public async createSite(testId: number, siteConfig?: Partial<SiteDocument>) {
    const siteDocument = { ...buildSiteDocument(testId), ...siteConfig };

    const database = await this.getDatabase();
    const { container } = await database.containers.createIfNotExists({
      id: this.coreContainerId,
      partitionKey: { paths: ['/docType'] },
    });
    await container.items.upsert(siteDocument);
    console.log(`Written site: ${siteDocument.id} to Cosmos DB.`);
  }

  public async deleteSite(testId: number) {
    const siteDocument = buildSiteDocument(testId);

    const database = await this.getDatabase();
    const { container } = await database.containers.createIfNotExists({
      id: this.coreContainerId,
      partitionKey: { paths: ['/docType'] },
    });
    try {
      await container.item(siteDocument.id, 'site').delete();
      console.log(`Deleted site: ${siteDocument.id} from Cosmos DB.`);
    } catch (e) {
      console.error(e);
    }
  }

  public async createUser(testId: number, roles: Role[]) {
    const userDocument = buildUserDocument(testId, roles);

    const database = await this.getDatabase();
    const { container } = await database.containers.createIfNotExists({
      id: this.coreContainerId,
      partitionKey: { paths: ['/docType'] },
    });
    await container.items.upsert(userDocument);
    console.log(`Written user: ${userDocument.id} to Cosmos DB.`);
  }

  public async deleteUser(testId: number) {
    const userDocument = buildUserDocument(testId, []);

    const database = await this.getDatabase();
    const { container } = await database.containers.createIfNotExists({
      id: this.coreContainerId,
      partitionKey: { paths: ['/docType'] },
    });
    try {
      await container.item(userDocument.id, 'user').delete();
      console.log(`Deleted user: ${userDocument.id} from Cosmos DB.`);
    } catch (e) {
      console.error(e);
    }
  }
}

export default CosmosDbClient;
