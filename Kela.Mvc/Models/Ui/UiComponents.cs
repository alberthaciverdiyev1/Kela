namespace Kela.Mvc.Models.Ui;

public class ButtonModel
{
    public string Text { get; set; } = string.Empty;
    public string Variant { get; set; } = "primary";  // primary | secondary | danger | ghost
    public string Type { get; set; } = "button";       // button | submit
    public string? OnClick { get; set; }
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

public class PageHeaderModel
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? CreateText { get; set; }
    public string CreateOnClick { get; set; } = "openCreate()";
    public bool Sortable { get; set; }
    public StatCardModel? StatCard { get; set; }
}

public class StatCardModel
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Accent { get; set; } = "bg-primary-light text-primary";
}
