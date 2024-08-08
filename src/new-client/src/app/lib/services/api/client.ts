import { cookies } from 'next/headers';

interface ClientOptions {
  baseUrl: string;
  token?: string;
}

class Client {
  private readonly baseUrl: string;

  public constructor({ baseUrl }: ClientOptions) {
    this.baseUrl = baseUrl;
  }

  public get<T = unknown>(
    path: string,
    cacheConfig?: RequestCache,
  ): Promise<T> {
    const tokenCookie = cookies().get('token');

    return fetch(`${this.baseUrl}/${path}`, {
      cache: cacheConfig,
      method: 'GET',
      headers: tokenCookie
        ? {
            Authorization: `Bearer ${tokenCookie.value}`,
          }
        : undefined,
    }).then(response => {
      if (response.status !== 200) {
        return undefined;
      }
      return response.json();
    });
  }

  public post<T = unknown>(path: string, payload: BodyInit): Promise<T> {
    const tokenCookie = cookies().get('token');

    return fetch(`${this.baseUrl}/${path}`, {
      method: 'POST',
      body: payload,
      headers: tokenCookie?.value
        ? {
            Authorization: `Bearer ${tokenCookie.value}`,
          }
        : undefined,
    }).then(response => {
      if (response.status !== 200) {
        return undefined;
      }
      return response.json();
    });
  }

  public getBaseUrl(): string {
    return this.baseUrl;
  }
}

export default Client;
