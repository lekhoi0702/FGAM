import { createRouter, createWebHashHistory } from 'vue-router'
import { routes } from './routes'
import { applyAuthGuard } from './guards'

export const router = createRouter({
  history: createWebHashHistory(import.meta.env.MODE === 'android' ? '/' : '/WarehouseAccess/'),
  routes
})

router.beforeEach(applyAuthGuard)
