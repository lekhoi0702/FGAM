import { createApp } from 'vue'
import { createWarehouseApp } from './app/create-app'
import 'sweetalert2/dist/sweetalert2.min.css'

createWarehouseApp(createApp).mount('#app')

if ('serviceWorker' in navigator) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register(`${import.meta.env.BASE_URL}sw.js`).catch(() => {})
  })
}
