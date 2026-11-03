/* Profil şəkli üçün kvadrat kırpma pəncərəsi.
   Blazor seçilmiş şəkli data URL kimi ötürür, istifadəçi sürüşdürüb yaxınlaşdırır,
   nəticə 512×512 JPEG kimi qaytarılır (yükləmə ölçüsü də kiçilir). */
(function () {
    if (window.edutorPhotoCrop) return;

    const OUTPUT = 512;

    function createOverlay(labels) {
        const wrap = document.createElement('div');
        wrap.className = 'epc-overlay';
        wrap.innerHTML = `
            <div class="epc-panel" role="dialog" aria-modal="true">
                <p class="epc-title"></p>
                <div class="epc-stage">
                    <canvas class="epc-canvas" width="320" height="320"></canvas>
                    <div class="epc-ring"></div>
                </div>
                <p class="epc-hint"></p>
                <label class="epc-zoom">
                    <span class="epc-zoom-label"></span>
                    <input type="range" min="1" max="3" step="0.01" value="1" />
                </label>
                <div class="epc-actions">
                    <button type="button" class="epc-cancel"></button>
                    <button type="button" class="epc-save"></button>
                </div>
            </div>`;
        wrap.querySelector('.epc-title').textContent = labels.title || '';
        wrap.querySelector('.epc-hint').textContent = labels.hint || '';
        wrap.querySelector('.epc-zoom-label').textContent = labels.zoom || '';
        wrap.querySelector('.epc-cancel').textContent = labels.cancel || '';
        wrap.querySelector('.epc-save').textContent = labels.save || '';
        return wrap;
    }

    function loadImage(dataUrl) {
        return new Promise((resolve, reject) => {
            const img = new Image();
            img.onload = () => resolve(img);
            img.onerror = reject;
            img.src = dataUrl;
        });
    }

    window.edutorPhotoCrop = {
        /// dataUrl → Promise<{ dataUrl, contentType } | null>
        async open(dataUrl, labels) {
            labels = labels || {};
            const img = await loadImage(dataUrl);

            const overlay = createOverlay(labels);
            document.body.appendChild(overlay);

            const canvas = overlay.querySelector('.epc-canvas');
            const ctx = canvas.getContext('2d');
            const zoomInput = overlay.querySelector('input[type=range]');

            const size = canvas.width;
            const fit = Math.min(size / img.width, size / img.height);

            let zoom = 1;
            let offsetX = 0;
            let offsetY = 0;
            let dragging = false;
            let startX = 0;
            let startY = 0;
            let originX = 0;
            let originY = 0;

            function scale() { return fit * zoom; }

            // Şəkil kvadratı tam örtsün deyə sürüşmə həddi.
            function clamp() {
                const scaled = scale();
                const maxX = Math.max(0, (img.width * scaled - size) / 2);
                const maxY = Math.max(0, (img.height * scaled - size) / 2);
                offsetX = Math.min(maxX, Math.max(-maxX, offsetX));
                offsetY = Math.min(maxY, Math.max(-maxY, offsetY));
            }

            function draw() {
                clamp();
                const scaled = scale();
                const w = img.width * scaled;
                const h = img.height * scaled;

                ctx.clearRect(0, 0, size, size);
                ctx.fillStyle = '#111';
                ctx.fillRect(0, 0, size, size);
                ctx.drawImage(img, (size - w) / 2 + offsetX, (size - h) / 2 + offsetY, w, h);
            }

            function render() { draw(); }

            canvas.addEventListener('pointerdown', (e) => {
                dragging = true;
                startX = e.clientX;
                startY = e.clientY;
                originX = offsetX;
                originY = offsetY;
                canvas.setPointerCapture(e.pointerId);
            });

            canvas.addEventListener('pointermove', (e) => {
                if (!dragging) return;
                offsetX = originX + (e.clientX - startX);
                offsetY = originY + (e.clientY - startY);
                render();
            });

            canvas.addEventListener('pointerup', (e) => {
                dragging = false;
                canvas.releasePointerCapture?.(e.pointerId);
            });

            zoomInput.addEventListener('input', () => {
                zoom = Number(zoomInput.value) || 1;
                render();
            });

            render();

            return await new Promise((resolve) => {
                const close = (value) => {
                    overlay.remove();
                    resolve(value);
                };

                overlay.querySelector('.epc-cancel').addEventListener('click', () => close(null));
                overlay.querySelector('.epc-save').addEventListener('click', () => {
                    // Kırpma çərçivəsi göstərilən kvadratdır — nəticə onu yüksək
                    // ölçüdə təkrar çəkirik ki, profil şəkli kəskin görünsün.
                    const out = document.createElement('canvas');
                    out.width = out.height = OUTPUT;

                    const scaled = scale();
                    const ratio = OUTPUT / size;
                    const w = img.width * scaled * ratio;
                    const h = img.height * scaled * ratio;

                    out.getContext('2d').drawImage(
                        img,
                        (OUTPUT - w) / 2 + offsetX * ratio,
                        (OUTPUT - h) / 2 + offsetY * ratio,
                        w, h);

                    close({ dataUrl: out.toDataURL('image/jpeg', 0.9), contentType: 'image/jpeg' });
                });
            });
        }
    };
})();
