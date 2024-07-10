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
import { GuardedRoute } from './Components/GuardedRoute';
import { Permissions } from './Types/Permissions';

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
                  <Route path="/availability" element={
                    <GuardedRoute permission={Permissions.GetAvailability}>
                      <ScheduleEditor />
                    </GuardedRoute>
                  } />
                  <Route path="/site" element={
                    <GuardedRoute permission={Permissions.SetSites}>
                      <EditSiteServicesCtx />
                    </GuardedRoute>} />
                  <Route path="/bookings" element={<DailyBookingsCtx />} />
                  <Route path="/calendar" element={<AppointmentsCalendarCtx />} />
                  <Route path="/templates" element={
                    <GuardedRoute permission={Permissions.GetAvailability}>
                      <TemplateListView />
                    </GuardedRoute>
                  } />
                  <Route path="/templates/edit" element={<WeekTemplateEditorCtx />} />
                  <Route path="/templates/edit/:templateId" element={<WeekTemplateEditorCtx />} />
                  <Route path="/unauthorised" element={<h2>You do not have permission to access this page.</h2>} />
            </Routes>
          </AppPage>
        </SiteContextProvider>
      </AuthContextProvider>
    </Router>
  );
}

export default App;
