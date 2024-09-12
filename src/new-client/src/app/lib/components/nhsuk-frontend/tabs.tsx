import { ReactNode } from 'react';

type Tab = {
  title: string;
  content: ReactNode;
};

type TabsProps = {
  title?: string;
  tabs: Tab[];
};

const Tabs = ({ tabs, title }: TabsProps) => {
  return (
    <div className="nhsuk-tabs" data-module="nhsuk-tabs">
      {title && <h2 className="nhsuk-tabs__title">{title}</h2>}

      <ul className="nhsuk-tabs__list">
        {tabs.map((tab, index) => {
          const tabId = `${tab.title.replace(/\s+/g, '-').toLowerCase()}-${index}`;

          return (
            <li
              className="nhsuk-tabs__list-item nhsuk-tabs__list-item--selected"
              key={tabId}
            >
              <a className="nhsuk-tabs__tab" href={`#${tabId}`}>
                {tab.title}
              </a>
            </li>
          );
        })}
      </ul>

      {tabs.map((tab, index) => {
        const tabId = `${tab.title.replace(/\s+/g, '-').toLowerCase()}-${index}`;

        return (
          <div className="nhsuk-tabs__panel" id={tabId} key={tabId} role="tab">
            {tab.content}
          </div>
        );
      })}
    </div>
  );
};

export default Tabs;
