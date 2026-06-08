import { ROUTE_NAMES } from '../constants/routes'
import LoginView from '../views/LoginView.vue'
import HomeView from '../views/HomeView.vue'
import EntryView from '../views/EntryView.vue'
import MonitorView from '../views/MonitorView.vue'
import HistoryView from '../views/HistoryView.vue'
import SettingsView from '../views/SettingsView.vue'
import CheckInMobileView from '../views/CheckInMobileView.vue'
import SettingsEmployeesView from '../views/SettingsEmployeesView.vue'
import SettingsFactoriesView from '../views/SettingsFactoriesView.vue'
import SettingsUserTypesView from '../views/SettingsUserTypesView.vue'
import SettingsDepartmentsView from '../views/SettingsDepartmentsView.vue'
import SettingsPurposesView from '../views/SettingsPurposesView.vue'
import TvMonitorView from '../views/TvMonitorView.vue'

export const routes = [
  { path: '/login', name: ROUTE_NAMES.login, component: LoginView, meta: { shell: false, title: 'Login' } },
  { path: '/', name: ROUTE_NAMES.home, component: HomeView, meta: { shell: true, title: 'Home' } },
  { path: '/entry', name: ROUTE_NAMES.entry, component: EntryView, meta: { shell: true, title: 'Entry' } },
  { path: '/monitor', name: ROUTE_NAMES.monitor, component: MonitorView, meta: { shell: true, title: 'Monitor' } },
  { path: '/history', name: ROUTE_NAMES.history, component: HistoryView, meta: { shell: true, title: 'History' } },
  { path: '/check-in-mobile', name: ROUTE_NAMES.checkInMobile, component: CheckInMobileView, meta: { shell: false, title: 'Check In Mobile' } },
  { path: '/tv-monitor', name: 'tv-monitor', component: TvMonitorView, meta: { shell: false, title: 'TV Monitor' } },
  { path: '/settings', name: ROUTE_NAMES.settings, component: SettingsView, meta: { shell: true, title: 'Settings' } },
  { path: '/settings/employees', name: ROUTE_NAMES.settingsEmployees, component: SettingsEmployeesView, meta: { shell: true, title: 'Settings - Employees' } },
  { path: '/settings/factories', name: ROUTE_NAMES.settingsFactories, component: SettingsFactoriesView, meta: { shell: true, title: 'Settings - Factories' } },
  { path: '/settings/user-types', name: ROUTE_NAMES.settingsUserTypes, component: SettingsUserTypesView, meta: { shell: true, title: 'Settings - User Types' } },
  { path: '/settings/departments', name: ROUTE_NAMES.settingsDepartments, component: SettingsDepartmentsView, meta: { shell: true, title: 'Settings - Departments' } },
  { path: '/settings/purposes', name: ROUTE_NAMES.settingsPurposes, component: SettingsPurposesView, meta: { shell: true, title: 'Settings - Purposes' } }
]
