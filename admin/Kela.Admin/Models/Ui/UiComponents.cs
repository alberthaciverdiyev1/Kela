namespace Kela.Admin.Models.Ui;

public class ButtonModel
{
    public string Text { get; set; } = string.Empty;
    public string Variant { get; set; } = "primary";  // primary | secondary | danger | ghost
    public string Type { get; set; } = "button";       // button | submit
    /// <summary>Qlobal funksiya adı (inline onclick yerinə).</summary>
    public string? Action { get; set; }

    /// <summary>Action üçün JSON arqumentlər, məs. ["students"].</summary>
    public string? Args { get; set; }

    /// <summary>Modalı bağlayan düymə (inline onclick yerinə).</summary>
    public bool Dismiss { get; set; }
    public string? Href { get; set; }
    public string? Id { get; set; }
}

public class InputModel
{
    public string Name { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string Type { get; set; } = "text";
    public string? Value { get; set; }
    public string? Placeholder { get; set; }
    public bool Required { get; set; }

    /// <summary>JS yoxlaması olmayan səhifələr üçün (məs. giriş) brauzerin öz required-ı.</summary>
    public bool NativeRequired { get; set; }

    public string? Id { get; set; }
}

public class SelectOption
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class SelectModel
{
    public string Name { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string? Value { get; set; }
    public List<SelectOption> Options { get; set; } = new();
    public bool Required { get; set; }

    /// <summary>Çoxseçimli select — seçilmiş variantlar <see cref="Values"/>-də verilir.</summary>
    public bool Multiple { get; set; }
    public List<string> Values { get; set; } = new();

    /// <summary>JS yoxlaması olmayan səhifələr üçün (məs. giriş) brauzerin öz required-ı.</summary>
    public bool NativeRequired { get; set; }

    public string? Id { get; set; }
}

public class SwitchModel
{
    public string Name { get; set; } = string.Empty;
    public string? Label { get; set; }
    public bool Checked { get; set; } = true;
    public string? Id { get; set; }
}

public class FileUploadModel
{
    public string Name { get; set; } = "file";
    public string? Id { get; set; }
    public string? Label { get; set; }
    public string? Accept { get; set; }
    public string? Hint { get; set; }
}

public class BulkBarModel
{
    public string Url { get; set; } = string.Empty;
    public bool CanDelete { get; set; } = true;
    public bool CanRestore { get; set; } = true;
    public bool CanActivate { get; set; }
    public bool CanDeactivate { get; set; }
}

public class EditorModel
{
    public string Name { get; set; } = "content";

    public string? Id { get; set; }

    public string Label { get; set; } = string.Empty;
}

public class ModalModel
{
    public string Id { get; set; } = "modal";

    /// <summary>Başlıq elementinin id-si — JS redaktə zamanı mətni dəyişir. Null → idsiz başlıq.</summary>
    public string? TitleId { get; set; } = "modalTitle";

    public string? Title { get; set; }

    public string WidthClass { get; set; } = "max-w-lg";

    /// <summary>Modal gövdəsi — adətən səhifənin öz formal partial-ı.</summary>
    public Microsoft.AspNetCore.Html.IHtmlContent? Body { get; set; }

    public bool ShowError { get; set; } = true;

    public string ErrorId { get; set; } = "formError";

    /// <summary>Başlıqda bağlama düyməsindən əvvəl göstərilən əlavə düymələr (məs. fullscreen).</summary>
    public Microsoft.AspNetCore.Html.IHtmlContent? HeaderExtra { get; set; }
}

public class TableColumnModel
{
    public string Label { get; set; } = string.Empty;

    /// <summary>Tailwind en sinfi (məs. "w-24").</summary>
    public string? WidthClass { get; set; }

    public bool Right { get; set; }
}

/// <summary>Cədvəl başlığı: seçim qutusu + # sütunu bütün səhifələrdə eyni qaydada.</summary>
public class TableHeadModel
{
    /// <summary>Seçim qutusu sütunu — yalnız toplu əməliyyatı olan cədvəllərdə.</summary>
    public bool Selectable { get; set; }

    public bool RowNumber { get; set; } = true;

    public List<TableColumnModel> Columns { get; set; } = new();
}

/// <summary>JS-ə id → ad xəritəsi ötürən kiçik data bloku.</summary>
public class OptionMapModel
{
    public string Name { get; set; } = string.Empty;

    public IReadOnlyDictionary<long, string> Items { get; set; } = new Dictionary<long, string>();
}

/// <summary>Standart siyahı cədvəli: loader, başlıq, boş tbody və footer bir yerdə.</summary>
public class TableModel
{
    public bool Selectable { get; set; }

    public List<TableColumnModel> Columns { get; set; } = new();

    public string BodyId { get; set; } = "rows";

    public bool Footer { get; set; } = true;
}

public class TableFooterModel
{
    public string InfoId { get; set; } = "tableInfo";

    public string PaginationId { get; set; } = "pagination";
}

/// <summary>Səhifə başlığı zolağı — başlıq özü topbar-dadır (ViewData["Title"]).</summary>
public class PageHeaderModel
{
    public string? CreateText { get; set; }
    public string CreateAction { get; set; } = "openCreate";
    public bool Sortable { get; set; }
    public StatCardModel? StatCard { get; set; }

    /// <summary>Yaratma düyməsindən əvvəl göstərilən əlavə düymələr (məs. Excel, silinənlər).</summary>
    public Microsoft.AspNetCore.Html.IHtmlContent? Extra { get; set; }
}

public class StatCardModel
{
    /// <summary>JS dəyəri doldurursa element id-si.</summary>
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public string Accent { get; set; } = "bg-primary-light text-primary";

    /// <summary>Server tərəfdən hazır dəyər (boşdursa JS doldurur).</summary>
    public string? Value { get; set; }

    public string ValueClass { get; set; } = "text-slate-950";

    /// <summary>İkonsuz, yığcam kart — çoxlu göstərici sıralandıqda.</summary>
    public bool Compact { get; set; }
}
