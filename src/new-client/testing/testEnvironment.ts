import dotenv from 'dotenv';
import path from 'path';

dotenv.config({ path: path.resolve(__dirname, 'playwright.env') });

const environment = {
  BASE_URL: process.env.BASE_URL ?? '',
  TEST_USER_USERNAME: process.env.TEST_USER_USERNAME ?? '',
  TEST_USER_PASSWORD: process.env.TEST_USER_PASSWORD ?? '',
};

export default environment;
