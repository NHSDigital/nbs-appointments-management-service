import dotenv from 'dotenv';
import path from 'path';

dotenv.config({ path: path.resolve(__dirname, 'playwright.env') });

const testUsers = {
  default: {
    username: process.env.TEST_USER_USERNAME ?? '',
    password: process.env.TEST_USER_PASSWORD ?? '',
  },
  // TODO: Expand with multiple users for different roles
  // auditer: { username: 'auditer', password: '1234abc' },
  // checkIn: { username: 'checkin', password: '1234abc' },
};

const environment = {
  BASE_URL: process.env.BASE_URL ?? '',
  TEST_USERS: testUsers,
};

export default environment;
