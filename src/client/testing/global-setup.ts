process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

import env from './testEnvironment';
import { promises as fs } from 'fs';
import * as path from 'path';
import { CosmosClient } from '@azure/cosmos';
import { type FullConfig } from '@playwright/test';

const { COSMOS_ENDPOINT, COSMOS_TOKEN, SEED_COSMOS_BEFORE_RUN } = env;

// eslint-disable-next-line @typescript-eslint/no-unused-vars
async function globalSetup(config: FullConfig) {
  if (SEED_COSMOS_BEFORE_RUN) {
    await seedCosmos();
  }
}

async function seedCosmos() {
  const client = new CosmosClient({
    endpoint: COSMOS_ENDPOINT,
    key: COSMOS_TOKEN,
  });

  const { database: appts } = await client.databases.createIfNotExists({
    id: 'appts',
  });

  const dbSeederDocumentsRoot = path.resolve(
    __dirname,
    '../../../data/CosmosDbSeeder/items/local',
  );
  const containerFolders = await fs.readdir(dbSeederDocumentsRoot, {
    withFileTypes: true,
  });

  containerFolders.forEach(async folder => {
    const containerName = folder.name;
    const { container } = await appts.containers.createIfNotExists({
      id: containerName,
    });

    const folderPath = path.join(dbSeederDocumentsRoot, containerName);
    const documents = await fs.readdir(folderPath);

    documents.forEach(async document => {
      const filePath = path.join(folderPath, document);
      const fileContent = await fs.readFile(filePath, 'utf-8');

      const jsonData = JSON.parse(fileContent);
      await container.items.upsert(jsonData);
    });
  });
}

export default globalSetup;
