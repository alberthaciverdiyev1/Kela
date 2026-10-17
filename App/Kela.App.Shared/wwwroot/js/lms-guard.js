/*
 * lms-guard.js — Kela tələbə tətbiqi üçün qlobal ekran qoruması (web).
 *
 * NƏ EDİR:  watermark · blur (fokus itkisi) · sürtünmə (sağ klik/copy/seçim/qısayol) · aşkarlama + report
 * NƏ ETMİR: OS səviyyəsində ekran yazılışını (OBS, Game Bar, telefon recorder) BLOKLAMIYIR — brauzerdə mümkün deyil.
 *           Əsl blok üçün native qat: NativeScreenGuard (FLAG_SECURE / iOS isCaptured).
 *
 * İstifadə (Blazor): window.lmsGuard.enable() / disable() / setWatermark(text) / registerCallback(dotNetRef)
 * Konfiqurasiya: script-dən ƏVVƏL window.LmsGuardConfig = { ... } təyin edin.
 */
(function () {
  'use strict';
  if (window.lmsGuard) return;

  const cfg = Object.assign({
    autoStart: true,             // DOM hazır olan kimi sürtünmə + aşkarlama aktivləşsin
    blockContextMenu: true,
    blockCopy: true,
    blockSelection: true,
    blockShortcuts: true,
    blockPrint: true,
    blurOnFocusLoss: true,
    detectDisplayCapture: true,
    watermark: true,
    watermarkClock: true       // watermark-da canlı tarix/saat göstərilsin
  }, window.LmsGuardConfig || {});

  const STYLE_ID = 'lms-guard-style';
  const WM_ID = 'lms-guard-watermark';

  let active = false;
  let callbacks = [];
  let blurTimer = null;

  // ------------------------------------------------------------------ helpers

  function isEditable(el) {
    if (!el || !el.tagName) return false;
    const t = el.tagName;
    return t === 'INPUT' || t === 'TEXTAREA' || t === 'SELECT' || el.isContentEditable === true;
  }

  function report(type) {
    const detail = { type: type, path: location.pathname, at: Date.now() };
    for (const ref of callbacks) {
      try { ref.invokeMethodAsync('OnViolation', type, location.pathname); } catch (e) { /* nəzərə alma */ }
    }
    try { window.dispatchEvent(new CustomEvent('lms-guard:violation', { detail: detail })); } catch (e) { /* IE */ }
  }

  // ------------------------------------------------------------------ style

  function injectStyle() {
    if (document.getElementById(STYLE_ID)) return;
    const s = document.createElement('style');
    s.id = STYLE_ID;
    s.textContent = `
      html.lms-guard, html.lms-guard body { -webkit-user-select: none; -moz-user-select: none; user-select: none; -webkit-touch-callout: none; }
      html.lms-guard input, html.lms-guard textarea, html.lms-guard select, html.lms-guard [contenteditable="true"] {
        -webkit-user-select: text; -moz-user-select: text; user-select: text;
      }
      html.lms-guard img, html.lms-guard video, html.lms-guard canvas, html.lms-guard svg { -webkit-user-drag: none; user-drag: none; }
      html.lms-guard-blur #app { filter: blur(18px) grayscale(20%); transition: filter .12s ease; pointer-events: none; -webkit-user-select: none; user-select: none; }
      #${WM_ID} {
        position: fixed; right: 10px; bottom: 84px; z-index: 2147483000; pointer-events: none;
        max-width: 70vw; text-align: right; white-space: pre-line;
        font-family: system-ui, -apple-system, sans-serif; font-size: 11px; line-height: 1.5;
        color: rgba(46,88,68,0.55); text-shadow: 0 0 3px rgba(255,255,255,0.65);
      }
      /* Desktop (lg): alt naviqasiya yoxdur, şaquli sidebar var → küncə qaytar */
      @media (min-width: 1024px) {
        #${WM_ID} { bottom: 12px; right: 16px; }
      }
    `;
    document.head.appendChild(s);
  }

  // ------------------------------------------------------------------ handlers

  function onContextMenu(e) { if (isEditable(e.target)) return; e.preventDefault(); report('contextmenu'); }
  function onCopy(e) { if (isEditable(e.target)) return; e.preventDefault(); report(e.type); }
  function onSelectStart(e) { if (isEditable(e.target)) return; e.preventDefault(); }
  function onDragStart(e) { e.preventDefault(); }

  function onKeyDown(e) {
    const k = (e.key || '').toLowerCase();
    let blocked = false;

    if (k === 'f12') {
      blocked = true;
    } else if (e.ctrlKey || e.metaKey) {
      if (k === 'p' || k === 's' || k === 'u') blocked = true;
      else if (e.shiftKey && (k === 'i' || k === 'j' || k === 'c' || k === 'k')) blocked = true;
    }

    if (blocked) {
      e.preventDefault();
      e.stopPropagation();
      report('shortcut:' + k);
      return false;
    }

    // PrintScreen: bəzi brauzerlər keydown ötürmür, ötürsə qeyd edirik (bloklamaq mümkün deyil).
    if (e.key === 'PrintScreen') report('shortcut:printscreen');
  }

  // Başqa tab/aplikasiyaya keçid və ya pəncərə fokus itkisi — avtomatik blur.
  function onVisibility() {
    if (document.hidden) { report('visibility-hidden'); blur(); }
    else { unblur(); }
  }
  function onBlur() { report('window-blur'); blur(); }
  function onFocus() { unblur(); }
  function onPageHide() { report('pagehide'); blur(); }
  function onPageShow() { unblur(); }

  function blur() {
    if (!cfg.blurOnFocusLoss) return;
    clearTimeout(blurTimer);
    blurTimer = setTimeout(() => document.documentElement.classList.add('lms-guard-blur'), 40);
  }
  function unblur() {
    clearTimeout(blurTimer);
    // Səhifə hələ də gizlidirsə açma (yalnız real qayıdışda təmizlənir).
    if (document.hidden) return;
    document.documentElement.classList.remove('lms-guard-blur');
  }

  // ------------------------------------------------------------- display capture

  function patchDisplayCapture() {
    if (!cfg.detectDisplayCapture) return;
    const md = navigator.mediaDevices;
    if (!md || typeof md.getDisplayMedia !== 'function' || md.getDisplayMedia.__lmsGuard) return;

    const original = md.getDisplayMedia.bind(md);
    const patched = async function () {
      report('display-capture-request');
      const stream = await original.apply(md, arguments);
      report('display-capture-start');
      try {
        stream.getVideoTracks().forEach(t =>
          t.addEventListener('ended', () => report('display-capture-stop')));
      } catch (e) { /* nəzərə alma */ }
      return stream;
    };
    patched.__lmsGuard = true;
    md.getDisplayMedia = patched;
  }

  // ------------------------------------------------------------------ watermark
  // TƏK watermark: sağ-aşağı küncdə, şagird + cihaz/vaxt məlumatları ilə (izləmə üçün).

  let baseLines = [];
  let clockTimer = null;

  function pad(n) { return n < 10 ? '0' + n : '' + n; }

  function formatNow() {
    const d = new Date();
    return pad(d.getDate()) + '.' + pad(d.getMonth() + 1) + '.' + d.getFullYear() + ' ' +
           pad(d.getHours()) + ':' + pad(d.getMinutes()) + ':' + pad(d.getSeconds());
  }

  function envInfo() {
    const parts = [];
    try { parts.push((navigator.userAgentData && navigator.userAgentData.platform) || navigator.platform || 'web'); } catch (e) { /* */ }
    try { if (screen && screen.width) parts.push(screen.width + 'x' + screen.height); } catch (e) { /* */ }
    try { const tz = Intl.DateTimeFormat().resolvedOptions().timeZone; if (tz) parts.push(tz); } catch (e) { /* */ }
    return parts.join(' · ');
  }

  function paint() {
    const el = document.getElementById(WM_ID);
    if (!el) return;
    const lines = baseLines.slice();
    const env = envInfo();
    if (env) lines.push(env);
    try { lines.push(location.pathname); } catch (e) { /* */ }
    if (cfg.watermarkClock) lines.push(formatNow());
    el.textContent = lines.join('\n');
  }

  function startClock() { if (!clockTimer) clockTimer = setInterval(paint, 1000); }
  function stopClock() { if (clockTimer) { clearInterval(clockTimer); clockTimer = null; } }

  function renderWatermark() {
    let el = document.getElementById(WM_ID);
    if (!cfg.watermark || baseLines.length === 0) {
      if (el) el.remove();
      stopClock();
      return;
    }
    if (!el) { el = document.createElement('div'); el.id = WM_ID; document.body.appendChild(el); }
    paint();
    if (cfg.watermarkClock) startClock(); else stopClock();
  }

  // -------------------------------------------------------------- enable/disable

  function enable() {
    if (active) return;
    active = true;
    injectStyle();
    document.documentElement.classList.add('lms-guard');

    if (cfg.blockContextMenu) document.addEventListener('contextmenu', onContextMenu, true);
    if (cfg.blockCopy) {
      document.addEventListener('copy', onCopy, true);
      document.addEventListener('cut', onCopy, true);
    }
    if (cfg.blockSelection) document.addEventListener('selectstart', onSelectStart, true);
    document.addEventListener('dragstart', onDragStart, true);
    if (cfg.blockShortcuts) document.addEventListener('keydown', onKeyDown, true);
    if (cfg.blockPrint) { try { window.print = function () { report('shortcut:p'); }; } catch (e) { /* nəzərə alma */ } }

    if (cfg.blurOnFocusLoss) {
      document.addEventListener('visibilitychange', onVisibility);
      window.addEventListener('blur', onBlur);
      window.addEventListener('focus', onFocus);
      window.addEventListener('pagehide', onPageHide);
      window.addEventListener('pageshow', onPageShow);
    }

    patchDisplayCapture();
    renderWatermark();
  }

  function disable() {
    if (!active) return;
    active = false;
    document.documentElement.classList.remove('lms-guard', 'lms-guard-blur');

    document.removeEventListener('contextmenu', onContextMenu, true);
    document.removeEventListener('copy', onCopy, true);
    document.removeEventListener('cut', onCopy, true);
    document.removeEventListener('selectstart', onSelectStart, true);
    document.removeEventListener('dragstart', onDragStart, true);
    document.removeEventListener('keydown', onKeyDown, true);
    document.removeEventListener('visibilitychange', onVisibility);
    window.removeEventListener('blur', onBlur);
    window.removeEventListener('focus', onFocus);
    window.removeEventListener('pagehide', onPageHide);
    window.removeEventListener('pageshow', onPageShow);

    stopClock();
    const el = document.getElementById(WM_ID);
    if (el) el.remove();
  }

  // ------------------------------------------------------------------ public API

  window.lmsGuard = {
    enable: enable,
    disable: disable,
    isEnabled: function () { return active; },
    setWatermark: function (text) {
      baseLines = text == null ? [] : String(text).split('\n').map(function (s) { return s.trim(); }).filter(Boolean);
      if (active) renderWatermark();
    },
    clearWatermark: function () {
      baseLines = [];
      stopClock();
      const el = document.getElementById(WM_ID);
      if (el) el.remove();
    },
    registerCallback: function (ref) { if (ref && callbacks.indexOf(ref) < 0) callbacks.push(ref); },
    unregisterCallback: function (ref) { callbacks = (ref ? callbacks.filter(c => c !== ref) : []); },
    report: report
  };

  function start() { if (cfg.autoStart) enable(); }
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start);
  else start();
})();
