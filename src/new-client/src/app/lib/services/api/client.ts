'use server';
import { cookies } from 'next/headers';
import redirectToIdServer from '../../../auth/redirectToIdServer';

interface ClientOptions {
  baseUrl: string;
  token?: string;
}

class Client {
  private readonly baseUrl: string;

  public constructor({ baseUrl }: ClientOptions) {
    this.baseUrl = baseUrl;
  }

  public get<T = unknown>(path: string, config?: RequestInit): Promise<T> {
    const tokenCookie = cookies().get('token');

    return fetch(`${this.baseUrl}/${path}`, {
      method: 'GET',
      headers: tokenCookie
        ? {
            Authorization: `Bearer ${tokenCookie.value}`,
          }
        : undefined,
      ...config,
    }).then(async response => {
      if (response.status === 401) {
        await redirectToIdServer();
        return undefined;
      }
      if (response.status === 403) {
        // TODO: Return error object with message
        // Implement error boundary to show "You may not see this resource" message
        // Also implement breadcrumbs so they can easily "go back a step" in one click
        throw new Error(
          '403 Forbidden: You lack the necessary permissions to view this resource',
        );
      }

      if (response.status === 200) {
        return response.json();
      }

      return undefined;
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
