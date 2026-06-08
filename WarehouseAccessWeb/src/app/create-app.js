import App from '../App.vue'
import '../styles/index.css'
import { router } from '../router'
import { restoreSession } from '../stores/auth.store'

export function createWarehouseApp(createApp) {
  restoreSession()
  const app = createApp(App)
  app.use(router)
  return app
}
