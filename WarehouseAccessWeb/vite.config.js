import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vuetify from 'vite-plugin-vuetify'
import fs from 'node:fs'
import path from 'node:path'
import { loadEnv } from 'vite'

const certDir = path.resolve(process.cwd(), '.cert')
const certFile = path.join(certDir, 'dev-cert.pem')
const keyFilePem = path.join(certDir, 'dev-key.pem')
const keyFileKey = path.join(certDir, 'dev-cert.key')
const pfxFile = path.join(certDir, 'dev-cert.pfx')
const pfxPasswordFile = path.join(certDir, 'dev-cert-password.txt')
const keyFile = fs.existsSync(keyFilePem) ? keyFilePem : keyFileKey
const hasHttpsFiles = fs.existsSync(certFile) && fs.existsSync(keyFile)
const hasPfxFile = fs.existsSync(pfxFile) && fs.existsSync(pfxPasswordFile)

const httpsConfig = hasHttpsFiles
  ? {
      cert: fs.readFileSync(certFile),
      key: fs.readFileSync(keyFile)
    }
  : hasPfxFile
    ? {
        pfx: fs.readFileSync(pfxFile),
        passphrase: fs.readFileSync(pfxPasswordFile, 'utf8').trim()
      }
    : false

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const isAndroid = mode === 'android'
  const devApiTarget = env.VITE_DEV_API_TARGET || 'http://localhost:5034/WarehouseAccessAPi'
  return {
    base: isAndroid ? '/' : '/WarehouseAccess/',
    plugins: [vue(), vuetify({ autoImport: true })],
    server: {
      host: true,
      https: httpsConfig,
      proxy: {
        '^/WarehouseAccess/(Auth|Companies|Departments|Purposes|Transactions|UserManagement|UserTypes)': {
          target: devApiTarget,
          changeOrigin: true,
          secure: false
        }
      }
    }
  }
})
