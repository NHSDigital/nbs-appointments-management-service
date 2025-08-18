import { ServerActionResult } from '@types';

/**
 * Converts the result of a server action into its return type, or throws.
 * We can't just throw from the server action directly because client components
 * cannot try catch server errors and errors within server actions aren't caught by error boundaries.
 * To correctly invoke an error boundary, we catch this result and throw here.
 */
const fromServer = async <T>(
  action: Promise<ServerActionResult<T>>,
): Promise<T> => {
  return action
    .then(
      result => {
        if (result.success) {
          return result.data;
        }
        throw new Error('Server action failed');
      },
      error => {
        throw error;
      },
    )
    .catch(error => {
      throw error;
    });
};

export default fromServer;
