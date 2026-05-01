/** Entrada decimal tolerante a coma; no fuerza `0` mientras se borra. */
export function normalizeDecimalTyping(raw: string): string {
  let s = raw.replace(',', '.')
  s = s.replace(/[^\d.]/g, '')
  const firstDot = s.indexOf('.')
  if (firstDot === -1) return s
  const rest = s.slice(firstDot + 1).replace(/\./g, '')
  return s.slice(0, firstDot + 1) + rest
}

export function parseDecimal(str: string): number | null {
  const t = str.trim().replace(',', '.')
  if (t === '' || t === '.' || t === '-.' || t === '-') return null
  const n = Number(t)
  return Number.isFinite(n) ? n : null
}

/** Solo dígitos (cantidades enteras no negativas). */
export function normalizeUnsignedIntTyping(raw: string): string {
  return raw.replace(/\D/g, '')
}

export function parseUnsignedInt(str: string): number | null {
  const t = str.trim().replace(/\D/g, '')
  if (t === '') return null
  const n = parseInt(t, 10)
  return Number.isFinite(n) ? n : null
}

/** Quita ceros a la izquierda salvo un solo "0". */
export function trimLeadingZerosInt(s: string): string {
  const d = normalizeUnsignedIntTyping(s)
  if (d === '') return ''
  const n = parseInt(d, 10)
  return Number.isNaN(n) ? '' : String(Math.max(0, n))
}

/** Quita ceros a la izquierda en la parte entera (ej. "01" → "1", "00.5" → "0.5"). */
export function trimLeadingZerosDecimal(s: string): string {
  const raw = s.trim().replace(',', '.')
  if (raw === '') return ''
  const norm = normalizeDecimalTyping(raw)
  if (norm === '') return ''
  const dot = norm.indexOf('.')
  if (dot === -1) {
    return norm.replace(/^0+(?=\d)/, '') || '0'
  }
  let intPart = norm.slice(0, dot)
  const frac = norm.slice(dot + 1)
  intPart = intPart.replace(/^0+(?=\d)/, '')
  if (intPart === '') intPart = '0'
  if (frac === '') return intPart
  return `${intPart}.${frac}`
}
