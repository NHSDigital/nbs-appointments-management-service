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

  public async get<T = unknown>(
    path: string,
    config?: RequestInit,
  ): Promise<ApiResponse<T>> {
    const cookieStore = await cookies();

    const tokenCookie = cookieStore.get('token');

    return fetch(`${this.baseUrl}/${path}`, {
      method: 'GET',
      headers: tokenCookie
        ? {
            Authorization: `Bearer ${tokenCookie.value}`,
          }
        : undefined,
      ...config,
    }).then(async response => {
      return await this.handleResponse<T>(response);
    });
  }

  public async post<T = unknown>(
    path: string,
    payload?: BodyInit,
    config?: RequestInit,
  ): Promise<ApiResponse<T>> {
    const cookieStore = await cookies();

    const tokenCookie = cookieStore.get('token');

    return fetch(`${this.baseUrl}/${path}`, {
      method: 'POST',
      body: payload,
      headers: tokenCookie?.value
        ? {
            Authorization: `Bearer ${tokenCookie.value}`,
          }
        : undefined,
      ...config,
    }).then(async response => {
      return await this.handleResponse<T>(response);
    });
  }

  public getBaseUrl(): string {
    return this.baseUrl;
  }

  private async handleResponse<T = unknown>(
    response: Response,
  ): Promise<ApiResponse<T>> {
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
        data: await this.extractResponseBody<T>(response),
      };
    }

    return {
      success: false,
      httpStatusCode: response.status,
      errorMessage: `$Unexpected status {response.status} from ${response.url}`,
    };
  }

  private async extractResponseBody<T>(response: Response): Promise<T | null> {
    const contentType = response.headers.get('Content-Type');

    switch (contentType) {
      case 'application/json':
        return response.json();
      case 'text/csv':
        // Responsibility on the caller to invoke this with <Blob> type
        return response.blob() as Promise<T>;
      default:
        return null;
    }
  }
}

export default Client;
