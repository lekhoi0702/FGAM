# Vue 3 + Vite

This template should help get you started developing with Vue 3 in Vite. The template uses Vue 3 `<script setup>` SFCs, check out the [script setup docs](https://v3.vuejs.org/api/sfc-script-setup.html#sfc-script-setup) to learn more.

Learn more about IDE Support for Vue in the [Vue Docs Scaling up Guide](https://vuejs.org/guide/scaling-up/tooling.html#ide-support).

## Mobile Camera Development

Mobile browsers require a trusted HTTPS origin for `getUserMedia`. Create a LAN certificate with `npm run cert:dev`, install/trust `.cert/dev-cert.cer` on the phone, then run `npm run dev:https` and open `https://<LAN-IP>:5173/check-in-mobile` on the device.

## Android Build + Sync (Capacitor)

1. Update API endpoint for Android in `.env.android`:

```env
VITE_API_BASE_URL=http://<YOUR-LAN-IP>:5111
```

2. Build web assets with Android env and sync native project:

```bash
npm run sync:android
```

3. Open Android Studio (optional):

```bash
npm run open:android
```

Notes:
- Phone/emulator must reach `http://<YOUR-LAN-IP>:5111`.
- Keep backend running and bound to `0.0.0.0:5111`.
