import dotenv from 'dotenv';
import path from 'path';

dotenv.config({ path: path.resolve(__dirname, 'playwright.env') });

const testUsers = {
  testUser1: {
    Username: process.env.TEST_USER_1_USERNAME ?? '',
    Password: process.env.TEST_USER_1_PASSWORD ?? '',
  },
  testUser2: {
    Username: process.env.TEST_USER_2_USERNAME ?? '',
    Password: process.env.TEST_USER_2_PASSWORD ?? '',
  },
  testUser3: {
    Username: process.env.TEST_USER_3_USERNAME ?? '',
    Password: process.env.TEST_USER_3_PASSWORD ?? '',
  },
  // This user should exist in the ID server but have no user document in our persistence
  testUser4: {
    Username: process.env.TEST_USER_4_USERNAME ?? '',
    Password: process.env.TEST_USER_4_PASSWORD ?? '',
  },
  testUser5: {
    Username: process.env.TEST_USER_5_USERNAME ?? '',
    Password: process.env.TEST_USER_5_PASSWORD ?? '',
  },
  testUser6: {
    Username: process.env.TEST_USER_6_USERNAME ?? '',
    Password: process.env.TEST_USER_6_PASSWORD ?? '',
  },
  adminTestUser: {
    Username: process.env.TEST_USER_7_USERNAME ?? '',
    Password: process.env.TEST_USER_7_PASSWORD ?? '',
  },
  testUser8: {
    Username: process.env.TEST_USER_8_USERNAME ?? '',
    Password: process.env.TEST_USER_8_PASSWORD ?? '',
  },
  testUser9: {
    Username: process.env.TEST_USER_9_USERNAME ?? '',
    Password: process.env.TEST_USER_9_PASSWORD ?? '',
  },
  testUser10: {
    Username: process.env.TEST_USER_10_USERNAME ?? '',
    Password: process.env.TEST_USER_10_PASSWORD ?? '',
  },
  testUser11: {
    Username: process.env.TEST_USER_11_USERNAME ?? '',
    Password: process.env.TEST_USER_11_PASSWORD ?? '',
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
