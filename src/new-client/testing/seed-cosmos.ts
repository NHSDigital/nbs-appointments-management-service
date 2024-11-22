process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

import env from './testEnvironment';
import { CosmosClient } from '@azure/cosmos';
import { test as setup } from '@playwright/test';

const { COSMOS_ENDPOINT, COSMOS_TOKEN } = env;

setup('Seed Cosmos', async ({}) => {
  const client = new CosmosClient({
    endpoint: COSMOS_ENDPOINT,
    key: COSMOS_TOKEN,
  });

  const { database: appts } = await client.databases.createIfNotExists({
    id: 'appts',
  });

  const { container: indexData } = await appts.containers.createIfNotExists({
    id: 'index_data',
  });

  const foo = await indexData.item('ABC01', 'site').read();
  console.dir(foo);
});
