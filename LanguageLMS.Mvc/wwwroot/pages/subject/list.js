(function () {
    window.removeItem = async function (id) {
        if (!await AdminUI.confirmDelete('Bu fənn silinəcək.')) return;
        const res = await AdminUI.post(`/Subject/Delete?id=${id}`, {});
        if (res.ok) { AdminUI.notify('Silindi'); AdminUI.reloadAfter(); }
        else AdminUI.error();
    };
})();
