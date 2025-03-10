export function debounce<A extends unknown[]>(
  fn: (...a: A) => void,
  time: number,
) {
  let timer: ReturnType<typeof setTimeout>;
  return function (...args: A) {
    clearTimeout(timer);
    timer = setTimeout(() => fn(...args), time);
  };
}
