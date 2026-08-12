/**
 * Controller — Davam (yoklama) səhifəsi.
 *
 * Klasik aylıq cədvəl: satırlar = şagirdlər, sütunlar = ayın günləri.
 * Hər xanaya klikləndikdə status seçim popoverı açılır; seçim ediləndə
 * həmin xana avtomatik saxlanılır (ayrıca "Saxla" düyməsi yoxdur).
 */
import Alpine from 'alpinejs';

export default function attendanceMonth(config) {
    return {
        // ── Konfiqurasiya ──────────────────────────────────────────────────
        workspaces: config.workspaces || [],
        workspaceId: config.workspaceId ? Number(config.workspaceId) : null,
        month: config.month || new Date().toISOString().slice(0, 7),

        // ── Vəziyyət ───────────────────────────────────────────────────────
        students: [],
        days: {},          // 'YYYY-MM-DD' → { studentId: status }
        dates: [],         // [{ day, iso, weekend }]
        loading: false,
        saveState: 'idle', // 'idle' | 'saving' | 'saved'

        // ── Status seçim popoverı ──────────────────────────────────────────
        showOptions: false,
        activeCell: null,  // { studentId, iso }
        menuLeft: 0,
        menuTop: 0,

        init() {
            this.buildDates();
            if (this.workspaceId) this.loadMonth();
        },

        // ── Tarix / gün hesablama ──────────────────────────────────────────

        buildDates() {
            const [y, m] = this.month.split('-').map(Number);
            const count = new Date(y, m, 0).getDate();
            this.dates = [];
            for (let d = 1; d <= count; d++) {
                const iso = `${y}-${String(m).padStart(2, '0')}-${String(d).padStart(2, '0')}`;
                const dow = new Date(y, m - 1, d).getDay(); // 0 = Bazar
                this.dates.push({ day: d, iso, weekend: dow === 0 || dow === 6 });
            }
        },

        shiftMonth(delta) {
            const [y, m] = this.month.split('-').map(Number);
            const d = new Date(y, m - 1 + delta, 1);
            this.month = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
            this.buildDates();
            this.loadMonth();
        },

        // ── Məlumat yükləmə / yazma ───────────────────────────────────────

        async loadMonth() {
            if (!this.workspaceId) {
                this.students = [];
                this.days = {};
                return;
            }
            this.loading = true;
            try {
                const res = await KelaApi('GET', `/api/v1/workspaces/${this.workspaceId}/attendance/month?month=${encodeURIComponent(this.month)}`);
                const data = res?.data ?? {};
                this.students = data.students || [];
                this.days = data.days || {};
            } catch (err) {
                window.alert(err.message);
            } finally {
                this.loading = false;
            }
        },

        /** Tək xananı avtomatik saxlayır (hər seçimdən sonra çağırılır). */
        async saveCell(iso) {
            if (!this.workspaceId) return;
            this.saveState = 'saving';
            try {
                await KelaApi('POST', `/api/v1/workspaces/${this.workspaceId}/attendance`, {
                    date: iso,
                    statuses: this.days[iso] || {},
                });
                this.saveState = 'saved';
                this.resetSaveState();
            } catch (err) {
                this.saveState = 'idle';
                window.alert(err.message);
            }
        },

        resetSaveState() {
            clearTimeout(this._saveTimer);
            this._saveTimer = setTimeout(() => {
                this.saveState = 'idle';
            }, 1800);
        },

        // ── Status əməliyyatları ───────────────────────────────────────────

        getStatus(studentId, iso) {
            return (this.days[iso]?.[studentId]) || 0;
        },

        openOptions(event, studentId, iso) {
            const rect = event.currentTarget.getBoundingClientRect();
            this.activeCell = { studentId, iso };
            // Popoverı xananın altında, ekran içində saxla.
            const menuWidth = 196;
            this.menuLeft = Math.max(menuWidth / 2, Math.min(rect.left + rect.width / 2, window.innerWidth - menuWidth / 2));
            this.menuTop = rect.bottom + 6;
            this.showOptions = true;
        },

        async selectStatus(studentId, iso, status) {
            if (!this.activeCell) return;
            this.days = {
                ...this.days,
                [iso]: { ...(this.days[iso] || {}), [studentId]: status },
            };
            this.showOptions = false;
            await this.saveCell(iso);
        },

        cellClass(studentId, iso) {
            const s = this.getStatus(studentId, iso);
            const base = 'hover:ring-1 hover:ring-base-content/30';
            switch (s) {
                case 1: return 'bg-success/20 text-success ' + base;
                case 2: return 'bg-error/20 text-error ' + base;
                case 3: return 'bg-warning/20 text-warning ' + base;
                case 4: return 'bg-info/20 text-info ' + base;
                default: return 'text-base-content/15 hover:bg-base-200 ' + base;
            }
        },

        cellTitle(studentId, iso) {
            const s = this.getStatus(studentId, iso);
            const labels = { 0: 'Qeyd yoxdur', 1: 'Gəldi', 2: 'Gəlmədi', 3: 'Gecikdi', 4: 'Üzrlü' };
            const name = this.students.find((st) => st.id === studentId)?.name || '';
            return `${iso} · ${name} — ${labels[s]}`;
        },
    };
}

// Alpine-də qeydiyyat: blade-də x-data="attendanceMonth(...)" işləyə bilsin.
Alpine.data('attendanceMonth', attendanceMonth);
// Alpine-i işə salır. Bu entry yalnız davam səhifəsində yüklənir.
Alpine.start();
