import CheckboxSelector from '@components/checkbox-selector';
import { AvailabilityBlock } from '@types';
import React from 'react';
import { services, serviceSummary } from '../../services';

type TimeBlockRowProps = {
  block: AvailabilityBlock;
};

const TimeBlockRow = ({ block }: TimeBlockRowProps) => {
  const uniqueId = 'test';
  const error = false;

  const [selectedBlock, setSelectedBlock] = React.useState({ ...block });

  const startTimeChanged = (time: string) => {
    setSelectedBlock({ ...selectedBlock, start: time });
  };

  const endTimeChanged = (time: string) => {
    setSelectedBlock({ ...selectedBlock, end: time });
  };

  const numbers = [4, 5, 6, 10, 12, 15];

  return (
    <tr role="row" className="nhsuk-table__row">
      <td
        className={`nhsuk-table__cell ${!!error ? 'nhsuk-form-group--error' : ''}`}
      >
        <div className="nhsuk-date-input__item">
          <input
            id={uniqueId + '_start'}
            type="time"
            className="nhsuk-input nhsuk-date-input nhsuk-input--width-5"
            value={selectedBlock.start}
            onChange={e => startTimeChanged(e.target.value)}
            aria-label="enter start time"
          />
        </div>
      </td>
      <td className="nhsuk-table__cell ">
        <div className="nhsuk-date-input__item">
          <input
            id={uniqueId + '_end'}
            type="time"
            className="nhsuk-input nhsuk-date-input nhsuk-input--width-5"
            value={selectedBlock.end}
            onChange={e => endTimeChanged(e.target.value)}
            aria-label="enter end time"
          />
        </div>
      </td>
      <td className="nhsuk-table__cell ">
        <CheckboxSelector
          defaultMessage="Select the current site available services, or add a break"
          options={services}
          summarise={serviceSummary}
        />
      </td>
      <td className="nhsuk-table__cell">
        <select
          key="b"
          className="nhsuk-select"
          name="max-simul"
          style={{ width: '120px' }}
        >
          {numbers.map(n => (
            <option key={n} value={n}>
              {n}
            </option>
          ))}
        </select>
      </td>
      <td className="nhsuk-table__cell">
        <div>
          <select
            key="c"
            className="nhsuk-select"
            name="max-simul"
            style={{ width: '64px' }}
          >
            {numbers.map(n => (
              <option key={n} value={n}>
                {n}
              </option>
            ))}
          </select>
          <span style={{ marginLeft: '32px' }}>
            Up to <b>12</b> appointments per hour
          </span>
        </div>
      </td>
      <td className="nhsuk-table__cell">
        <button className="nhsuk-button--link" type="button">
          Remove
        </button>
      </td>
    </tr>
  );
};

export default TimeBlockRow;
