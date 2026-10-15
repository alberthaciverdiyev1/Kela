namespace Kela.Mvc.Models.Ui;

/// <summary>Admin siyahı səhifələrinin vahid filtr paneli (Dərslər səhifəsindəki ilə eyni görünüş).</summary>
public class FilterBarModel
{
    // Axtarış sahəsi (sola tam yayılan)
    public bool Search { get; set; } = true;
    public string SearchPlaceholder { get; set; } = string.Empty;

    // Filtr select-ləri (soldan sağa sıra ilə)
    public List<FilterSelectModel> Selects { get; set; } = new();

    // "Silinənlər" düyməsi
    public bool Trash { get; set; }
    public string TrashId { get; set; } = "trashBtn";
    public string TrashText { get; set; } = string.Empty;
    public string TrashOnClick { get; set; } = "toggleTrash()";

    // Əsas düymə
    public bool ShowButton { get; set; } = true;
    public string ButtonText { get; set; } = string.Empty;
    public string ButtonOnClick { get; set; } = "loadList(1)";
}

public class FilterSelectModel
{
    public SelectModel Select { get; set; } = new();
    public string WidthClass { get; set; } = "w-44";
}
