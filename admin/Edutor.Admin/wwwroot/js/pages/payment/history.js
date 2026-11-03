// Ödəniş tarixçəsi — qeydi silmək.
window.deletePayment = async function (id) {
    const ok = await AdminUI.confirmDelete(L["Payment_DeleteConfirm"]);
    if (!ok) return;

    const res = await AdminUI.del(`/payment/payments/${id}`);
    if (res.ok) location.reload();
    else AdminUI.error(AdminUI.errText(res));
};
