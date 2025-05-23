import { ReadonlyRequestCookies } from 'next/dist/server/web/spec-extension/adapters/request-cookies';
import { cookies } from 'next/headers';
import { getCookieConsent, setCookieConsent } from '@services/cookiesService';
import { DayJsType, parseToUkDatetime, ukNow } from '@services/timeService';

jest.mock('next/headers');
const cookiesMock = cookies as jest.Mock<ReadonlyRequestCookies>;
const cookieStoreMock: ReadonlyRequestCookies = {
  get: jest.fn(),
  getAll: jest.fn(),
  has: jest.fn(),
  [Symbol.iterator]: jest.fn(),
  size: 1,
  set: jest.fn(),
  delete: jest.fn(),
};

jest.mock('@services/timeService', () => {
  const originalModule = jest.requireActual('@services/timeService');
  return {
    ...originalModule,
    ukNow: jest.fn(),
  };
});
const mockUkNow = ukNow as jest.Mock<DayJsType>;

jest.mock('@constants', () => ({
  LATEST_CONSENT_COOKIE_VERSION: 5,
}));

describe('Cookies service', () => {
  beforeEach(() => {
    jest.resetAllMocks();
  });

  it('can access and parse consent cookies', async () => {
    const getMock = jest.fn().mockImplementation((name: string) => {
      if (name === 'nhsuk-mya-cookie-consent') {
        return {
          value: '%7B%22consented%22%3Atrue%2C%22version%22%3A2%7D',
        };
      }
      return undefined;
    });

    cookiesMock.mockReturnValue({
      ...cookieStoreMock,
      get: getMock,
    });

    const result = await getCookieConsent();

    expect(result).toEqual({
      consented: true,
      version: 2,
    });

    expect(getMock).toHaveBeenCalledWith('nhsuk-mya-cookie-consent');
  });

  it('returns undefined if the consent cookie is not set', async () => {
    cookiesMock.mockReturnValue({
      ...cookieStoreMock,
      get: jest.fn().mockReturnValue(undefined),
    });

    const result = await getCookieConsent();

    expect(result).toBeUndefined();
  });

  it('can record cookie consent', async () => {
    const setMock = jest.fn();
    cookiesMock.mockReturnValue({
      ...cookieStoreMock,
      set: setMock,
    });

    mockUkNow.mockReturnValue(parseToUkDatetime('2024-03-24T08:34:00'));

    await setCookieConsent(true);

    expect(setMock).toHaveBeenCalledWith(
      'nhsuk-mya-cookie-consent',
      '%7B%22consented%22%3Atrue%2C%22version%22%3A5%7D',
      {
        expires: parseToUkDatetime('2025-03-24T08:34:00').toDate(),
      },
    );
  });

  it('can record cookie non-consent', async () => {
    const setMock = jest.fn();
    cookiesMock.mockReturnValue({
      ...cookieStoreMock,
      set: setMock,
    });

    mockUkNow.mockReturnValue(parseToUkDatetime('2021-01-31T13:29:31'));

    await setCookieConsent(false);

    expect(setMock).toHaveBeenCalledWith(
      'nhsuk-mya-cookie-consent',
      '%7B%22consented%22%3Afalse%2C%22version%22%3A5%7D',
      {
        expires: parseToUkDatetime('2022-01-31T13:29:31').toDate(),
      },
    );
  });
});
