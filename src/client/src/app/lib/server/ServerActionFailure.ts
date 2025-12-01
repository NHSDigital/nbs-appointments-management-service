import { notAuthenticated, notAuthorized } from '@services/authService';
import { logError } from '@services/logService';
import { ApiErrorResponse } from '@types';
import { notFound, redirect } from 'next/navigation';

type ServerActionFailureType =
  | ServerActionExceptionParams
  | ServerActionHttpFailureParams
  | ServerActionRedirectParams
  | ServerActionFeatureToggleDisabledParams
  | ServerActionForbiddenParams;

type ServerActionExceptionParams = {
  type: 'Exception';
  error: Error;
};

type ServerActionHttpFailureParams = {
  type: 'HttpFailure';
  httpStatusCode: number;
  url: string;
};

type ServerActionRedirectParams = {
  type: 'Redirect';
  redirectUrl: string;
};

type ServerActionFeatureToggleDisabledParams = {
  type: 'FeatureToggleDisabled';
  toggleName: string;
};

type ServerActionForbiddenParams = {
  type: 'ForbiddenAction';
};

class ServerActionFailure {
  public readonly details: ServerActionFailureType;

  constructor(details: ServerActionFailureType) {
    this.details = details;
  }

  public handle() {
    switch (this.details.type) {
      case 'Redirect':
        return redirect(this.details.redirectUrl);

      case 'Exception':
        logError(
          'An exception occurred in a server action',
          this.details.error,
        );
        throw this.details.error;

      case 'HttpFailure': {
        switch (this.details.httpStatusCode) {
          case 404:
            return notFound();
          case 401:
            return notAuthenticated();
          case 403:
            return notAuthorized();
          default:
            logError(
              `An HTTP failure occurred in a server action: ${this.details.httpStatusCode} for URL ${this.details.url}`,
            );
            throw new Error(
              `An HTTP failure occurred in a server action: ${this.details.httpStatusCode} for URL ${this.details.url}`,
            );
        }
      }

      case 'FeatureToggleDisabled':
        logError(
          `A server action was attempted for a disabled feature toggle: ${this.details.toggleName}`,
        );
        return notFound();

      default:
        logError(
          'An unpredicted ServerActionFailure was handled.',
          undefined,
          this.details,
        );
        throw new Error('An unpredicted ServerActionFailure was handled.');
    }
  }
}

class ServerActionException extends ServerActionFailure {
  constructor(errorMessage: string) {
    super({ type: 'Exception', error: new Error(errorMessage) });
  }
}

class ServerActionHttpFailure extends ServerActionFailure {
  constructor(apiError: ApiErrorResponse) {
    super({
      type: 'HttpFailure',
      httpStatusCode: apiError.httpStatusCode,
      url: apiError.url,
    });
  }
}

class ServerActionRedirect extends ServerActionFailure {
  constructor(redirectUrl: string) {
    super({ type: 'Redirect', redirectUrl: redirectUrl });
  }
}

class ServerActionToggleDisabled extends ServerActionFailure {
  constructor(toggleName: string) {
    super({ type: 'FeatureToggleDisabled', toggleName: toggleName });
  }
}

class ServerActionForbidden extends ServerActionFailure {
  constructor() {
    super({ type: 'ForbiddenAction' });
  }
}

export {
  ServerActionFailure,
  ServerActionToggleDisabled,
  ServerActionHttpFailure,
  ServerActionException,
  ServerActionRedirect,
  ServerActionForbidden,
};
