import type { ReactNode } from 'react';

export interface TabConfig {
  key: string;
  label: string;
  icon?: ReactNode;
  content: ReactNode;
}

export interface TabContainerProps {
  tabs: TabConfig[];
  activeTab: string;
  onTabChange: (tabKey: string) => void;
  className?: string;
}

export const TabContainer = ({
  tabs,
  activeTab,
  onTabChange,
  className = ""
}: TabContainerProps) => {
  const activeTabContent = tabs.find(tab => tab.key === activeTab)?.content;

  return (
    <div className={`space-y-6 ${className}`}>
      {/* Tab Buttons */}
      <div className="bg-continental-gray-3 p-1 rounded-md w-full">
        <div className="flex w-full">
          {tabs.map((tab) => (
            <button
              key={tab.key}
              onClick={() => onTabChange(tab.key)}
              className={`flex items-center justify-center gap-2 px-4 py-2 rounded-md transition-colors w-1/2 ${
                activeTab === tab.key
                  ? 'bg-white text-continental-black shadow-sm'
                  : 'bg-transparent text-continental-gray-1 hover:text-continental-black'
              }`}
            >
              {tab.icon}
              <span>{tab.label}</span>
            </button>
          ))}
        </div>
      </div>

      {/* Tab Content */}
      <div className="tab-content">
        {activeTabContent}
      </div>
    </div>
  );
};