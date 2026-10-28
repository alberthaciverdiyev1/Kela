// Ödəniş tarixçəsi — qeydi silmək.
window.deletePayment = async function (id) {
    const ok = await AdminUI.confirmDelete('Bu ödəniş qeydi silinsin? Vade tarixi yenidən hesablanacaq.');
    if (!ok) return;

    const res = await AdminUI.del(`/payment/payments/${id}`);
    if (res.ok) location.reload();
    else AdminUI.error(AdminUI.errText(res));
};
