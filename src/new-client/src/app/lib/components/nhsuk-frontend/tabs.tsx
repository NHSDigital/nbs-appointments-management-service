'use client';
import { ReactNode, useState } from 'react';
import Button from './button';

type Tab = {
  title: string;
  content: ReactNode;
};

type TabsProps = {
  title?: string;
  tabs: Tab[];
};

const Tabs = ({ tabs, title }: TabsProps) => {
  const tabsWithIds = tabs.map((tab, index) => {
    const tabId = `${tab.title.replace(/\s+/g, '-').toLowerCase()}-${index}`;
    return { ...tab, id: tabId };
  });
  const [selectedTab, setSelectedTab] = useState(tabsWithIds[0].id);

  return (
    <div className="nhsuk-tabs" data-module="nhsuk-tabs">
      {title && <h2 className="nhsuk-tabs__title">{title}</h2>}

      <ol className="nhsuk-list nhsuk-u-clear nhsuk-u-margin-0">
        {tabsWithIds.map((tab, index) => {
          return (
            <li
              key={`button-list-${index}`}
              className="nhsuk-u-margin-bottom-0 nhsuk-u-float-left nhsuk-a-to-z-min-width"
              style={{ borderBottom: 0 }}
            >
              <Button
                key={tab.id}
                onClick={() => setSelectedTab(tab.id)}
                selected={selectedTab === tab.id}
                style={{
                  margin: 0,
                  background: selectedTab === tab.id ? 'white' : 'lightgrey',
                  color: 'black',
                  borderRadius: 0,
                  borderBottom: 0,
                  textDecoration: 'none',
                  fontStyle: 'normal',
                  fontWeight: 'normal',
                  paddingLeft: '2rem',
                  paddingRight: '2rem',
                }}
              >
                {tab.title}
              </Button>
            </li>
          );
        })}
      </ol>

      <div className="nhsuk-tabs__panel" id="configuration-tabs" role="tab">
        {tabsWithIds.find(tab => tab.id === selectedTab)?.content}
      </div>
    </div>
  );
};

export default Tabs;
