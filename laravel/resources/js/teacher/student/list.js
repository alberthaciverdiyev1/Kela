export default function createStudentList(fragmentUrl) {
    return {

        async refresh(tableEl) {
            try {
                tableEl.innerHTML = await KelaFragment(fragmentUrl);
            } catch (err) {
                window.alert(err.message);
            }
        },
    };
}
