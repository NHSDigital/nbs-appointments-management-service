class UnauthorisedError extends Error {
  public readonly type: string;

  constructor(message = 'Forbidden: You lack the necessary permissions') {
    super(message);
    this.name = 'UnauthorisedError';
    this.type = 'UnauthorisedError';
  }
}

export default UnauthorisedError;
