import { Fieldset } from 'nhsuk-react-components';
import { ReactNode } from 'react';

type Props = {
  legend?: string;
  children: ReactNode;
};

const FieldSet = ({ legend, children }: Props) => {
  return (
    <Fieldset>
      {legend && (
        <Fieldset.Legend>
          <h1 className="nhsuk-fieldset__heading">{legend}</h1>
        </Fieldset.Legend>
      )}
      {children}
    </Fieldset>
  );
};

export default FieldSet;
