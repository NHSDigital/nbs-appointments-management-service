'use client';
import React from 'react';
import styles from './styles.module.css';

type SelectorProps = {
  defaultMessage: string;
  selectAllMessage?: string;
  options: { key: string; value: string }[];
  defaultOptions?: string[];
  summarise: (opts: string[]) => string | undefined;
  onChange: (opts: string[]) => void;
};

const CheckboxSelector = ({
  defaultMessage,
  options,
  defaultOptions,
  summarise,
  onChange,
  selectAllMessage,
}: SelectorProps) => {
  const [isVisible, setIsVisible] = React.useState(false);
  const [selectedOptions, setSelectedOptions] = React.useState(
    defaultOptions ?? ([] as string[]),
  );
  const containerRef = React.useRef<HTMLDivElement>(null);
  const hasError = false;
  const uniqueId = 'test';

  const handleSelectAll = () => {
    if (selectedOptions.length !== options.length) {
      setSelectedOptions(options.map(opt => opt.key));
    } else {
      setSelectedOptions([] as string[]);
    }
  };

  const handleChange = (opt: string) => {
    if (selectedOptions.includes(opt)) {
      setSelectedOptions(selectedOptions.filter(i => i !== opt));
    } else {
      setSelectedOptions([...selectedOptions, opt]);
    }
  };

  const summary = React.useMemo(
    () => summarise(selectedOptions),
    [selectedOptions, summarise],
  );

  const allSelected = React.useMemo(
    () => selectedOptions.length === options.length,
    [selectedOptions, options],
  );

  React.useEffect(() => {
    onChange(selectedOptions);
  }, [selectedOptions, onChange]);

  React.useEffect(() => {
    const handleEventOutside = (event: Event) => {
      if (
        containerRef.current &&
        !containerRef.current.contains(event.target as Node)
      ) {
        setIsVisible(false);
      }
    };
    document.addEventListener('mousedown', handleEventOutside);
    document.addEventListener('focusin', handleEventOutside);
    return () => {
      document.removeEventListener('mousedown', handleEventOutside);
      document.removeEventListener('focusin', handleEventOutside);
    };
  }, [containerRef]);

  return (
    <div
      className={
        styles['dropdown-check-list'] + ` ${isVisible ? styles.visible : ''}`
      }
      // eslint-disable-next-line jsx-a11y/no-noninteractive-tabindex
      tabIndex={0}
      onFocus={() => setIsVisible(true)}
      ref={containerRef}
    >
      <span className={styles.anchor + ` ${hasError ? styles.error : ''}`}>
        {summary ? summary : defaultMessage}
      </span>
      <ul className={styles.items}>
        <li className="nhsuk-checkboxes__item">
          <input
            type="checkbox"
            id={`selectAllCheckbox_${uniqueId}`}
            value="0"
            className="nhsuk-checkboxes__input"
            onChange={handleSelectAll}
            checked={allSelected}
          />
          <label
            htmlFor={`selectAllCheckbox_${uniqueId}`}
            className="nhsuk-checkboxes__label"
          >
            {selectAllMessage ?? 'Select all'}
          </label>
        </li>
        {options.map(opt => {
          const key = `${opt.key}_${uniqueId}`;
          return (
            <li key={key} className="nhsuk-checkboxes__item">
              <input
                type="checkbox"
                id={key}
                value={opt.key}
                name={opt.key}
                checked={selectedOptions.includes(opt.key)}
                onChange={() => handleChange(opt.key)}
                className="nhsuk-checkboxes__input"
              />
              <label htmlFor={key} className="nhsuk-checkboxes__label">
                {opt.value}
              </label>
            </li>
          );
        })}
      </ul>
    </div>
  );
};

export default CheckboxSelector;
