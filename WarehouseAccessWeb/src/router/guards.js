import { useAuthState } from '../stores/auth.store'

const PUBLIC_PATHS = new Set(['/login', '/tv-monitor'])

export function applyAuthGuard(to, from, next) {
  const auth = useAuthState()
  
  // Normalize path: convert to lowercase and strip trailing slash for tolerance
  const normalizedPath = to.path.toLowerCase().replace(/\/$/, '')
  const isPublic = PUBLIC_PATHS.has(normalizedPath)

  if (!auth.isAuthenticated && !isPublic) {
    next({ path: '/login', query: { redirect: to.fullPath } })
    return
  }

  if (auth.isAuthenticated && to.path === '/login') {
    next('/monitor')
    return
  }

  if (auth.isAuthenticated && to.path.startsWith('/settings')) {
    const status = auth.currentUser?.recordStatus || auth.currentUser?.RecordStatus
    if (String(status) !== '2') {
      next('/monitor')
      return
    }
  }

  next()
}
