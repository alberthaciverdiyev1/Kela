(function () {
    window.removeItem = async function (id) {
        if (!await AdminUI.confirmDelete('Bu dərs silinəcək.')) return;
        const res = await AdminUI.post(`/Lesson/Delete?id=${id}`, {});
        if (res.ok) { AdminUI.notify('Silindi'); AdminUI.reloadAfter(); }
        else AdminUI.error();
    };
})();
