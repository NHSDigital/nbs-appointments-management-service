import dotenv from 'dotenv';
import path from 'path';

dotenv.config({ path: path.resolve(__dirname, 'playwright.env') });

const environment = {
  BASE_URL: process.env.BASE_URL ?? '',
  SEED_COSMOS_BEFORE_RUN: process.env.SEED_COSMOS_BEFORE_RUN === 'true',
  COSMOS_ENDPOINT: process.env.COSMOS_ENDPOINT ?? '',
  COSMOS_TOKEN: process.env.COSMOS_TOKEN ?? '',
  BUILD_NUMBER: process.env.BUILD_NUMBER ?? '',
};

export default environment;
