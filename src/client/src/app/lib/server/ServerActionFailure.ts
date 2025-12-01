import { notAuthenticated, notAuthorized } from '@services/authService';
import { logError } from '@services/logService';
import { notFound, redirect } from 'next/navigation';

class ServerActionFailure {
  public readonly Message: string;
  public readonly StatusCode?: number;
  public readonly Error?: Error;
  public readonly RedirectRequired?: string;

  constructor(
    message: string,
    statusCode?: number,
    error?: Error,
    redirectRequired?: string,
  ) {
    this.Message = message;
    this.StatusCode = statusCode;
    this.Error = error;
    this.RedirectRequired = redirectRequired;
  }

  public handle() {
    if (this.RedirectRequired !== undefined) {
      return redirect(this.RedirectRequired);
    }

    if (this.Error !== undefined) {
      logError('An error occurred in a server action', this.Error, {
        statusCode: `${this.StatusCode}`,
        message: this.Message,
      });
      throw this.Error;
    }

    if (this.StatusCode === 404) {
      return notFound();
    }
    if (this.StatusCode === 401) {
      return notAuthenticated();
    }
    if (this.StatusCode === 403) {
      return notAuthorized();
    }

    logError('An unpredicted ServerActionFailure was handled.', undefined, {
      statusCode: `${this.StatusCode}`,
      message: this.Message,
    });

    throw new Error('An unpredicted ServerActionFailure was handled.');
  }
}

class ServerActionException extends ServerActionFailure {
  constructor(errorMessage: string) {
    super(errorMessage, undefined, new Error(errorMessage));
  }
}

class ServerActionHttpFailure extends ServerActionFailure {
  constructor(message: string, statusCode: number) {
    super(message, statusCode, undefined);
  }
}

class ServerActionRedirect extends ServerActionFailure {
  constructor(redirectUrl: string) {
    super(
      'A server action has prompted a redirect',
      undefined,
      undefined,
      redirectUrl,
    );
  }
}

export {
  ServerActionFailure,
  ServerActionHttpFailure,
  ServerActionException,
  ServerActionRedirect,
};
