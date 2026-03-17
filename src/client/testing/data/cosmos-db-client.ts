/* eslint-disable no-console */
import { CosmosClient } from '@azure/cosmos';
import { SiteDocument, UserDocument } from '@e2etests/types';

class CosmosDbClient {
  private readonly client: CosmosClient;

  private readonly coreContainerId = 'core_data';
  private readonly bookingContainerId = 'booking_data';
  private readonly indexContainerId = 'index_data';

  constructor(cosmosEndpoint: string, cosmosToken: string) {
    this.client = new CosmosClient({
      endpoint: cosmosEndpoint,
      key: cosmosToken,
      connectionPolicy: {
        retryOptions: {
          maxRetryAttemptCount: 100,
          fixedRetryIntervalInMilliseconds: 100,
          maxWaitTimeInSeconds: 60,
        },
      },
    });
  }

  private async getDatabase() {
    const { database: appts } = await this.client.databases.createIfNotExists({
      id: 'appts',
    });

    return appts;
  }

  public async createSite(siteDocument: SiteDocument) {
    const database = await this.getDatabase();
    const { container } = await database.containers.createIfNotExists({
      id: this.coreContainerId,
      partitionKey: { paths: ['/docType'] },
    });
    await container.items.upsert(siteDocument);
    console.log(`Written site: ${siteDocument.id} to Cosmos DB.`);
  }

  public async deleteSite(site: SiteDocument | undefined) {
    if (site !== undefined) {
      const database = await this.getDatabase();
      const { container } = await database.containers.createIfNotExists({
        id: this.coreContainerId,
        partitionKey: { paths: ['/docType'] },
      });
      try {
        await container.item(site.id, 'site').delete();
        console.log(`Deleted site: ${site.id} from Cosmos DB.`);
      } catch (e) {
        console.error(e);
      }
    }
  }

  public async createUser(userDocument: UserDocument) {
    const database = await this.getDatabase();
    const { container } = await database.containers.createIfNotExists({
      id: this.coreContainerId,
      partitionKey: { paths: ['/docType'] },
    });
    await container.items.upsert(userDocument);
    console.log(
      `Written user: ${userDocument.id} with EULA v${userDocument.latestAcceptedEulaVersion} to Cosmos DB.`,
    );
  }

  public async deleteUser(user: UserDocument | undefined) {
    if (user !== undefined) {
      const database = await this.getDatabase();
      const { container } = await database.containers.createIfNotExists({
        id: this.coreContainerId,
        partitionKey: { paths: ['/docType'] },
      });
      try {
        await container.item(user.id, 'user').delete();
        console.log(`Deleted user: ${user.id} from Cosmos DB.`);
      } catch (e) {
        console.error(e);
      }
    }
  }
}

export default CosmosDbClient;
