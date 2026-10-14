async function removeLesson(id) {
    if (!await AdminUI.confirmDelete('Bu dərs silinəcək.')) return;
    const res = await AdminUI.del(`/lesson/${id}`);
    if (res.ok) { AdminUI.notify('Silindi'); location.reload(); }
    else AdminUI.error();
}
