import messages from './en/messages.json';

type Messages = typeof messages;

export function t(key: string, values: Record<string, unknown> = {}): string {
  const count = typeof values.count === 'number' ? values.count : undefined;
  let pluralForm = '';

  const hasCount = count !== undefined && count > 0;

  if (hasCount) pluralForm = count === 1 ? 'one' : 'other';

  const lookupKey = hasCount ? `${key}.${pluralForm}` : key;
  const raw = (messages as Messages)[lookupKey as keyof Messages];
  const template = typeof raw === 'string' ? raw : key;

  return template.replace(/{(\w+)}/g, (_, varName) => {
    const v = values[varName];
    return v === undefined ? `{${varName}}` : String(v);
  });
}
