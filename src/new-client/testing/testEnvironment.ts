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
};

const environment = {
  BASE_URL: process.env.BASE_URL ?? '',
  TEST_USERS: testUsers,
};

export default environment;
