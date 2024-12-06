import dotenv from 'dotenv';
import path from 'path';

dotenv.config({ path: path.resolve(__dirname, 'playwright.env') });

const testUsers = {
  testUser1: {
    username: process.env.TEST_USER_1_USERNAME ?? '',
    password: process.env.TEST_USER_1_PASSWORD ?? '',
  },
  testUser2: {
    username: process.env.TEST_USER_2_USERNAME ?? '',
    password: process.env.TEST_USER_2_PASSWORD ?? '',
  },
  testUser3: {
    username: process.env.TEST_USER_3_USERNAME ?? '',
    password: process.env.TEST_USER_3_PASSWORD ?? '',
  },
  // This user should exist in the ID server but have no user document in our persistence
  testUser4: {
    username: process.env.TEST_USER_4_USERNAME ?? '',
    password: process.env.TEST_USER_4_PASSWORD ?? '',
  },
  testUser5: {
    username: process.env.TEST_USER_5_USERNAME ?? '',
    password: process.env.TEST_USER_5_PASSWORD ?? '',
  },
};

const environment = {
  BASE_URL: process.env.BASE_URL ?? '',
  SEED_COSMOS_BEFORE_RUN: process.env.SEED_COSMOS_BEFORE_RUN === 'true',
  COSMOS_ENDPOINT: process.env.COSMOS_ENDPOINT ?? '',
  COSMOS_TOKEN: process.env.COSMOS_TOKEN ?? '',
  TEST_USERS: testUsers,
};

export default environment;
