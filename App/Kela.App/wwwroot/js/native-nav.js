/*
 * native-nav.js — Android geri tuşu/jesti Blazor route yığınına bağlanır.
 *
 * WebView'da geri tuşu varsayılan olarak aktiviti bağlayır (tətbiq çıxır).
 * Burada Blazor-un history.pushState çağırışları sayılır; geri tuşu basıldıqda
 * yığın varsa bir addım geri gedirik, yoxdursa native tərəf tətbiqi bağlayır.
 */
(function () {
  'use strict';
  if (window.kelaNative) return;

  let depth = 0;

  const pushState = history.pushState.bind(history);
  const replaceState = history.replaceState.bind(history);

  // Yalnız pushState sayılır: replaceState ilkin yükləmədə də çağırılır.
  history.pushState = function (state, title, url) {
    depth++;
    return pushState(state, title, url);
  };
  history.replaceState = function (state, title, url) {
    return replaceState(state, title, url);
  };

  window.addEventListener('popstate', function () {
    if (depth > 0) depth--;
  });

  // Native deep link → Blazor NavigationManager (MainLayout-də [JSInvokable] Navigate).
  window.kelaNative = {
    registerNavigation: function (ref) {
      window.kelaNavigate = function (path) {
        return ref.invokeMethodAsync('Navigate', path);
      };
    },
    canGoBack: function () {
      return depth > 0;
    },
    handleBack: function () {
      if (depth > 0) {
        history.back();
        return true;
      }
      return false;
    }
  };
})();
