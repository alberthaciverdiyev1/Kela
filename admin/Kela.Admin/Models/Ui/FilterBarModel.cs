namespace Kela.Admin.Models.Ui;

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
    public string TrashAction { get; set; } = "toggleTrash";

    // Sıralama seçimi (verilərsə sağda göstərilir)
    public FilterSortModel? Sort { get; set; }

    // Əsas düymə
    public bool ShowButton { get; set; } = true;
    public string ButtonText { get; set; } = string.Empty;
    public string ButtonAction { get; set; } = "loadList";
    public string ButtonArgs { get; set; } = "[1]";
}

public class FilterSortModel
{
    public string Id { get; set; } = "sort";

    public string Value { get; set; } = "created_desc";

    /// <summary>Əlavə variantlar (məs. "order_asc" — əl ilə sıra).</summary>
    public List<SelectOption> Extra { get; set; } = new();
}

public class FilterSelectModel
{
    public SelectModel Select { get; set; } = new();
    public string WidthClass { get; set; } = "w-44";
}
