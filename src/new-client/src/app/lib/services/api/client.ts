'use server';
import { cookies } from 'next/headers';
import { ApiResponse } from '@types';

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
    config?: RequestInit,
  ): Promise<ApiResponse<T>> {
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
        return {
          success: false,
          httpStatusCode: 401,
          errorMessage:
            'Unauthorized: You must be logged in to view this resource',
        };
      }

      if (response.status === 403) {
        return {
          success: false,
          httpStatusCode: 403,
          errorMessage: 'Forbidden: You lack the necessary permissions',
        };
      }

      if (response.status === 404) {
        return {
          success: false,
          httpStatusCode: 404,
          errorMessage: 'Not found',
        };
      }

      if (response.status === 200) {
        return {
          success: true,
          data: await response.json(),
        };
      }

      throw new Error('An unhandled error occured');
    });
  }

  public post<T = unknown>(
    path: string,
    payload: BodyInit,
  ): Promise<ApiResponse<T>> {
    const tokenCookie = cookies().get('token');

    return fetch(`${this.baseUrl}/${path}`, {
      method: 'POST',
      body: payload,
      headers: tokenCookie?.value
        ? {
            Authorization: `Bearer ${tokenCookie.value}`,
          }
        : undefined,
    }).then(async response => {
      if (response.status === 401) {
        return {
          success: false,
          httpStatusCode: 401,
          errorMessage:
            'Unauthorized: You must be logged in to view this resource',
        };
      }

      if (response.status === 403) {
        return {
          success: false,
          httpStatusCode: 403,
          errorMessage: 'Forbidden: You lack the necessary permissions',
        };
      }

      if (response.status === 404) {
        return {
          success: false,
          httpStatusCode: 404,
          errorMessage: 'Not found',
        };
      }

      if (response.status === 200) {
        return {
          success: true,
          data: await response.json(),
        };
      }

      throw new Error('An unhandled error occured');
    });
  }

  public getBaseUrl(): string {
    return this.baseUrl;
  }
}

export default Client;
