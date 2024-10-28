class MyaError extends Error {
  public readonly digest: string;

  constructor(message: string, errorType: ErrorType = 'Error') {
    super(message);
    this.name = 'MyaError';
    this.digest = errorType;
  }
}

class UnauthorisedError extends MyaError {
  constructor() {
    super('Unauthorised', 'UnauthorizedError');
  }
}

export { MyaError, UnauthorisedError };
export default MyaError;
export type ErrorType = 'UnauthorizedError' | 'Error';
