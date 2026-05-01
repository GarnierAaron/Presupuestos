const KEY = 'presupuestos_device_id'

export function getOrCreateDeviceId(): string {
  try {
    let id = localStorage.getItem(KEY)
    if (!id) {
      id = crypto.randomUUID()
      localStorage.setItem(KEY, id)
    }
    return id
  } catch {
    return 'web-' + Math.random().toString(36).slice(2)
  }
}
