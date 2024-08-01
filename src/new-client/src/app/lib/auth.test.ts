import { cookies } from 'next/headers';
import { fetchUserProfile, fetchAccessToken } from './auth';

jest.mock('next/headers');
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const cookiesMock = cookies as jest.Mock<any>;

describe('fetchUserProfile', () => {
  it('returns undefined when no cookie provided', async () => {
    const cookiesMockObj = { get: jest.fn() };
    cookiesMock.mockReturnValue(cookiesMockObj);
    const result = await fetchUserProfile();
    expect(result).toBeUndefined();
  });

  it('returns undefined when api returns non success', async () => {
    const cookieGet = jest.fn();
    cookieGet.mockReturnValue({ value: 'data' });
    const cookiesMockObj = { get: cookieGet };
    cookiesMock.mockReturnValue(cookiesMockObj);

    const mockFetch = jest.fn();
    global.fetch = mockFetch;

    mockFetch.mockReturnValue({ status: 500 });

    const result = await fetchUserProfile();
    expect(result).toBeUndefined();
  });

  it('returns profile when api returns data', async () => {
    const cookieGet = jest.fn();
    cookieGet.mockReturnValue({ value: 'data' });
    const cookiesMockObj = { get: cookieGet };
    cookiesMock.mockReturnValue(cookiesMockObj);

    const mockFetch = jest.fn();
    global.fetch = mockFetch;

    mockFetch.mockReturnValue({
      status: 200,
      json: () => Promise.resolve({ emailAddress: 'test@test.com' }),
    });

    const result = await fetchUserProfile();
    expect(result?.emailAddress).toBe('test@test.com');
  });

  it('sets token cookie', async () => {
    const cookieSet = jest.fn();
    const cookiesMockObj = { set: cookieSet };
    cookiesMock.mockReturnValue(cookiesMockObj);
    const mockFetch = jest.fn();
    global.fetch = mockFetch;
    mockFetch.mockReturnValue({
      status: 200,
      json: () => Promise.resolve({ token: 'tokendata' }),
    });

    await fetchAccessToken('test_code');
    expect(cookieSet).toHaveBeenCalledWith('token', 'tokendata');
  });
});
