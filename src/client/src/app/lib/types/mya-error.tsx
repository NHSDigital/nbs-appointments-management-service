class MyaError extends Error {
  public readonly digest: string;

  constructor(message: string, errorType: ErrorType = 'Error') {
    super(message);
    this.name = 'MyaError';
    this.digest = errorType;
  }
}

class UnauthorizedError extends MyaError {
  constructor() {
    super('Unauthorized', 'UnauthorizedError');
  }
}

export { MyaError, UnauthorizedError };
export default MyaError;
export type ErrorType = 'UnauthorizedError' | 'Error';
