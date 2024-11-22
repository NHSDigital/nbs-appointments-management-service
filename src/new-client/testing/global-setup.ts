process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

import env from './testEnvironment';
import { promises as fs } from 'fs';
import * as path from 'path';
import { CosmosClient } from '@azure/cosmos';
import { type FullConfig } from '@playwright/test';

const { COSMOS_ENDPOINT, COSMOS_TOKEN } = env;

// eslint-disable-next-line @typescript-eslint/no-unused-vars
async function globalSetup(config: FullConfig) {
  const client = new CosmosClient({
    endpoint: COSMOS_ENDPOINT,
    key: COSMOS_TOKEN,
  });

  const { database: appts } = await client.databases.createIfNotExists({
    id: 'appts',
  });

  const dbSeederDocumentsRoot = path.resolve(
    __dirname,
    '../../../mock-data/CosmosDbSeeder/items',
  );
  const containerFolders = await fs.readdir(dbSeederDocumentsRoot, {
    withFileTypes: true,
  });

  for (const containerFolder of containerFolders) {
    const containerName = containerFolder.name;
    const { container } = await appts.containers.createIfNotExists({
      id: containerName,
    });

    const folderPath = path.join(dbSeederDocumentsRoot, containerName);
    const documents = await fs.readdir(folderPath);

    for (const document of documents) {
      const filePath = path.join(folderPath, document);
      const fileContent = await fs.readFile(filePath, 'utf-8');

      const jsonData = JSON.parse(fileContent.replace(/\s/g, ''));
      await container.items.upsert(jsonData);
    }
  }
}

export default globalSetup;
