/* eslint-disable no-console */
import { CosmosClient } from '@azure/cosmos';
import {
  BookingDocument,
  BookingIndexDocument,
  DailyAvailabilityDocument,
  SiteDocument,
  UserDocument,
} from '@e2etests/types';

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

  public async createBookings(bookingDocuments: BookingDocument[]) {
    const database = await this.getDatabase();
    const { container: bookingContainer } =
      await database.containers.createIfNotExists({
        id: this.bookingContainerId,
        partitionKey: { paths: ['/site'] },
      });

    const { container: indexContainer } =
      await database.containers.createIfNotExists({
        id: this.indexContainerId,
        partitionKey: { paths: ['/docType'] },
      });

    const upsertTasks = bookingDocuments.map(async bookingDocument => {
      await bookingContainer.items.upsert(bookingDocument);
      console.log(`Written booking: ${bookingDocument.id} to Cosmos DB.`);

      const indexDocument = this.mapToIndexDocument(bookingDocument);

      await indexContainer.items.upsert(indexDocument);
      console.log(`Written booking index: ${indexDocument.id} to Cosmos DB.`);
    });

    await Promise.all(upsertTasks);
  }

  public async deleteAllBookings(bookings: BookingDocument[]) {
    const database = await this.getDatabase();
    const { container: bookingContainer } =
      await database.containers.createIfNotExists({
        id: this.bookingContainerId,
        partitionKey: { paths: ['/site'] },
      });

    const { container: indexContainer } =
      await database.containers.createIfNotExists({
        id: this.indexContainerId,
        partitionKey: { paths: ['/docType'] },
      });

    const deleteTasks = bookings.map(async booking => {
      try {
        await bookingContainer.item(booking.id, booking.site).delete();
        console.log(`Deleted booking: ${booking.id} from Cosmos DB.`);

        await indexContainer.item(booking.id, 'booking_index').delete();
        console.log(`Deleted booking index: ${booking.id} from Cosmos DB.`);
      } catch (e) {
        console.error(e);
      }
    });

    Promise.all(deleteTasks);
  }

  public mapToIndexDocument = (
    bookingDocument: BookingDocument,
  ): BookingIndexDocument => {
    return {
      docType: 'booking_index',
      id: bookingDocument.id,
      reference: bookingDocument.reference,
      site: bookingDocument.site,
      from: bookingDocument.from,
      status: bookingDocument.status,
      nhsNumber: bookingDocument.attendeeDetails.nhsNumber,
      created: bookingDocument.created,
      statusUpdated: bookingDocument.statusUpdated,
    };
  };

  public async createAvailability(
    dailyAvailabilityDocuments: DailyAvailabilityDocument[],
  ) {
    const database = await this.getDatabase();
    const { container } = await database.containers.createIfNotExists({
      id: this.bookingContainerId,
      partitionKey: { paths: ['/site'] },
    });

    const upsertTasks = dailyAvailabilityDocuments.map(
      async dailyAvailabilityDocument => {
        await container.items.upsert(dailyAvailabilityDocument);
        console.log(
          `Written daily availability: ${dailyAvailabilityDocument.id} for site: ${dailyAvailabilityDocument.site} to Cosmos DB.`,
        );
      },
    );

    await Promise.all(upsertTasks);
  }

  public async deleteAvailability(availability: DailyAvailabilityDocument[]) {
    const database = await this.getDatabase();
    const { container: bookingContainer } =
      await database.containers.createIfNotExists({
        id: this.bookingContainerId,
        partitionKey: { paths: ['/site'] },
      });

    const deleteTasks = availability.map(async date => {
      try {
        await bookingContainer.item(date.id, date.site).delete();
        console.log(
          `Deleted daily availability: ${date.id} for site: ${date.site} from Cosmos DB.`,
        );
      } catch (e) {
        console.error(e);
      }
    });

    Promise.all(deleteTasks);
  }
}

export default CosmosDbClient;
