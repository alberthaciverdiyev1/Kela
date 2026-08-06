
export const COLOR_NAMES = ['primary', 'secondary', 'success', 'warning', 'error', 'info']

export const COLOR_LABELS = {
  primary: 'Ana Renk',
  secondary: 'İkincil Renk',
  success: 'Başarı (Success)',
  warning: 'Uyarı (Warning)',
  error: 'Hata (Error)',
  info: 'Bilgi (Info)',
}

function hexToRgb(hex) {
  const h = hex.replace('#', '')
  const full = h.length === 3 ? h.split('').map(c => c + c).join('') : h
  return {
    r: parseInt(full.slice(0, 2), 16),
    g: parseInt(full.slice(2, 4), 16),
    b: parseInt(full.slice(4, 6), 16),
  }
}

function rgbToHex({ r, g, b }) {
  const to = v => Math.max(0, Math.min(255, Math.round(v))).toString(16).padStart(2, '0')
  return `#${to(r)}${to(g)}${to(b)}`
}

function mix(a, b, weight) {
  const ca = hexToRgb(a)
  const cb = hexToRgb(b)
  return rgbToHex({
    r: ca.r + (cb.r - ca.r) * weight,
    g: ca.g + (cb.g - ca.g) * weight,
    b: ca.b + (cb.b - ca.b) * weight,
  })
}

export function deriveShades(baseHex) {
  return {
    50: mix(baseHex, '#ffffff', 0.90),
    100: mix(baseHex, '#ffffff', 0.80),
    200: mix(baseHex, '#ffffff', 0.60),
    300: mix(baseHex, '#ffffff', 0.40),
    400: mix(baseHex, '#ffffff', 0.20),
    500: baseHex,
    600: mix(baseHex, '#000000', 0.15),
    700: mix(baseHex, '#000000', 0.30),
    800: mix(baseHex, '#000000', 0.45),
    900: mix(baseHex, '#000000', 0.60),
  }
}

export const DEFAULT_BASES = {
  primary: '#4f46e5',    // indigo
  secondary: '#64748b',  // slate
  success: '#22c55e',    // green
  warning: '#f59e0b',    // amber
  error: '#ef4444',      // red
  info: '#0ea5e9',       // sky
}

export const DEFAULT_THEME = Object.fromEntries(
  Object.entries(DEFAULT_BASES).map(([name, base]) => [name, deriveShades(base)]),
)

export function applyTheme(colors, root = document.documentElement) {
  for (const name of COLOR_NAMES) {
    const palette = colors[name] || DEFAULT_THEME[name]
    for (const [scale, value] of Object.entries(palette)) {
      root.style.setProperty(`--kela-${name}-${scale}`, value)
      root.style.setProperty(`--p-${name}-${scale}`, value)
    }
    root.style.setProperty(`--kela-${name}`, palette['500'])
    root.style.setProperty(`--p-${name}-color`, palette['500'])
    root.style.setProperty(`--p-${name}-contrast-color`, '#ffffff')
  }

  const p = colors.primary || DEFAULT_THEME.primary
  root.style.setProperty('--p-primary-hover-color', p['600'])
  root.style.setProperty('--p-primary-active-color', p['700'])
  root.style.setProperty('--p-primary-emphasis', p['500'])
  root.style.setProperty('--p-primary-subtle-color', p['100'])
  root.style.setProperty('--p-primary-subtle-hover-color', p['200'])
  root.style.setProperty('--p-primary-subtle-active-color', p['300'])

  for (const name of ['success', 'warning', 'error', 'info']) {
    const pal = colors[name] || DEFAULT_THEME[name]
    root.style.setProperty(`--p-${name}-soft-color`, pal['100'])
    root.style.setProperty(`--p-${name}-soft-hover-color`, pal['200'])
    root.style.setProperty(`--p-${name}-soft-active-color`, pal['300'])
  }
}
