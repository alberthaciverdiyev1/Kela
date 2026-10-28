/*
 * pull-refresh.js — aşağı çəkərək yeniləmə.
 *
 * WebView-da native "pull to refresh" yoxdur, ona görə jesti özümüz tuturuq.
 * Yalnız səhifə ən yuxarıdaykən (scrollTop === 0) işə düşür.
 *
 * Blazor tərəfi window.edutorPull.register(dotNetRef) ilə qeydiyyatdan keçir;
 * hədd keçiləndə "OnPullRefresh" metodu çağırılır və spinner o cavab gələndən
 * sonra (lakin ən azı 450ms) gizlədilir.
 */
(function () {
  'use strict';
  if (window.edutorPull) return;

  var THRESHOLD = 72;   // bu qədər dartılıb buraxılarsa yeniləmə sayılır
  var MAX = 116;        // indikatorun maksimum hündürlüyü
  var MIN_SHOW = 450;   // spinner-in minimum görünmə müddəti

  var dotNet = null;
  var el = null;
  var startY = 0;
  var distance = 0;
  var tracking = false;
  var busy = false;

  var style = document.createElement('style');
  style.textContent =
    '@keyframes edutor-pull-spin{to{transform:rotate(360deg)}}' +
    '@media (prefers-reduced-motion: reduce){#edutor-pull div{animation:none !important}}';
  document.head.appendChild(style);

  function indicator() {
    if (el) return el;

    el = document.createElement('div');
    el.id = 'edutor-pull';
    el.setAttribute('aria-hidden', 'true');
    el.style.cssText = [
      'position:fixed', 'top:0', 'left:0', 'right:0', 'height:0',
      'display:flex', 'align-items:flex-end', 'justify-content:center',
      'overflow:hidden', 'pointer-events:none', 'z-index:60',
      'transition:height .22s ease-out'
    ].join(';');

    var ring = document.createElement('div');
    ring.style.cssText = [
      'margin-bottom:10px', 'width:26px', 'height:26px', 'border-radius:9999px',
      'border:2px solid #CFE3D9', 'border-top-color:#27815E', 'background:#fff',
      'box-shadow:0 2px 8px rgba(23,77,58,.14)'
    ].join(';');

    el.appendChild(ring);
    document.body.appendChild(el);
    return el;
  }

  function ring() { return el ? el.firstElementChild : null; }

  function scrollTop() {
    return window.scrollY || document.documentElement.scrollTop || document.body.scrollTop || 0;
  }

  function reset() {
    if (!el) return;
    el.style.height = '0px';
    var r = ring();
    if (r) { r.style.animation = ''; r.style.transform = 'rotate(0deg)'; }
  }

  function run() {
    busy = true;
    var e = indicator();
    e.style.transition = 'height .18s ease-out';
    e.style.height = '58px';

    var r = ring();
    if (r) { r.style.transform = ''; r.style.animation = 'edutor-pull-spin .7s linear infinite'; }

    var started = Date.now();
    var done = function () {
      var wait = Math.max(0, MIN_SHOW - (Date.now() - started));
      setTimeout(function () {
        busy = false;
        e.style.transition = 'height .22s ease-out';
        reset();
      }, wait);
    };

    if (dotNet) {
      // Yeniləmə bitəndən sonra gizlət; xəta olsa da gizlətməliyik.
      dotNet.invokeMethodAsync('OnPullRefresh').then(done, done);
    } else {
      done();
    }
  }

  document.addEventListener('touchstart', function (ev) {
    if (busy || ev.touches.length !== 1 || scrollTop() > 0) return;
    tracking = true;
    distance = 0;
    startY = ev.touches[0].clientY;
  }, { passive: true });

  document.addEventListener('touchmove', function (ev) {
    if (!tracking) return;

    // Səhifə aşağı sürüşübsə jest yeniləmə sayılmır.
    if (scrollTop() > 0) { tracking = false; reset(); return; }

    var dy = ev.touches[0].clientY - startY;
    if (dy <= 0) { distance = 0; reset(); return; }

    // Müqavimət: dartdıqca indikator daha yavaş böyüyür.
    distance = Math.min(MAX, dy * 0.55);

    indicator().style.height = distance + 'px';
    var r = ring();
    if (r) r.style.transform = 'rotate(' + Math.round(distance * 3) + 'deg)';

    // WebView-un öz overscroll/rubber-band effektini söndür.
    if (ev.cancelable) ev.preventDefault();
  }, { passive: false });

  document.addEventListener('touchend', function () {
    if (!tracking) return;
    tracking = false;
    if (distance >= THRESHOLD) run();
    else reset();
  });

  document.addEventListener('touchcancel', function () {
    if (!tracking) return;
    tracking = false;
    reset();
  });

  window.edutorPull = {
    register: function (ref) { dotNet = ref; },
    unregister: function () { dotNet = null; }
  };
})();
