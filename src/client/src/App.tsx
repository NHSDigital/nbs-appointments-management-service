import './App.css';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { SiteContextProvider } from './ContextProviders/SiteContextProvider';
import { EditSiteServicesCtx } from './Views/EditSiteServices';
import { AuthContextProvider } from './ContextProviders/AuthContextProvider';
import { DailyBookingsCtx } from './Views/DailyBookings';
import { HomePage } from './Views/HomePage';
import { AppointmentsCalendarCtx } from './Views/AppointmentCalendar';
import { WeekTemplateEditorCtx } from './Views/WeekTemplateEditor';
import { TemplateListView } from './Views/TemplateListView';
import { ScheduleEditor } from './Views/ScheduleEditor';
import { AppPage } from './Components/AppPage';
import { GuardedRoute } from './Components/GuardedRouteHOC';

function App() {
  return (
    <Router>
      <AuthContextProvider>
        <SiteContextProvider>
          <AppPage navLinks={[
          {name: "Home", route: "/"},
          {name: "Availability Scheduler", route:"/availability"},
          {name: "Templates", route:"/templates"},
          {name: "Edit Services", route: "/site"},
          {name: "Daily Bookings", route:"/bookings"},
          {name: "Calendar", route:"/calendar"}
        ]}>
            <Routes>
                  <Route path="/" element={<HomePage />} />
                  <Route path="/availability" element={<ScheduleEditor />} />
                  <Route path="/site" element={<EditSiteServicesCtx />} />
                  <Route path="/bookings" element={<DailyBookingsCtx />} />
                  <Route path="/calendar" element={<AppointmentsCalendarCtx />} />
                  <Route path="/templates" element={
                    <GuardedRoute permission='availability:get-setup'>
                      <TemplateListView />
                    </GuardedRoute>
                  } />
                  <Route path="/templates/edit" element={<WeekTemplateEditorCtx />} />
                  <Route path="/templates/edit/:templateId" element={<WeekTemplateEditorCtx />} />
            </Routes>
          </AppPage>
        </SiteContextProvider>
      </AuthContextProvider>
    </Router>
  );
}

export default App;
