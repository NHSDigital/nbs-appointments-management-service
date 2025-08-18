import { ServerActionResult } from '@types';

const asServerActionResult = <T>(result: T): ServerActionResult<T> => {
  return {
    success: true,
    data: result,
  };
};

export default asServerActionResult;
