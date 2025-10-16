import { logError } from '@services/logService';
import { ServerActionResult } from '@types';

/**
 * A wrapper for invoking server actions.
 * Converts the result of a server action into its return type, or throws.
 *
 * @remarks
 * Errors thrown by server actions during rendering will invoke a {@link https://nextjs.org/docs/app/api-reference/file-conventions/error | NextJS error boundary}.
 * Errors thrown by server actions as the result of imperative actions (such as button clicks or form submissions), do NOT.
 * NextJS in fact swallows these errors to avoid exposing system information, and the UI simply hangs.
 * The NextJS docs recommend returning errors as a bespoke type, then handling this with client logic.
 * They expect showing an error component, but in our case we don't have designs for that. We want to invoke the full error boundary instead.
 * To achieve this, we catch failed server action results here then throw an error from the receiving client component.
 *
 * @see {@link https://nextjs.org/docs/app/getting-started/error-handling | NextJS docs on error handling}
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
        logError('A server action result did not indicate success');
        throw new Error('Server action failed');
      },
      error => {
        logError('An unexpected error occurred in a server action', error);
        throw error;
      },
    )
    .catch(error => {
      logError('An unexpected error occurred in a server action', error);
      throw error;
    });
};

export default fromServer;
