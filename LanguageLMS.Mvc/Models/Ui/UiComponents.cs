namespace LanguageLMS.Mvc.Models.Ui;

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
