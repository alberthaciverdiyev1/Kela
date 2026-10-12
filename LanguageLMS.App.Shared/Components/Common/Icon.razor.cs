using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Components.Common;

public partial class Icon
{
    [Parameter] public string Name { get; set; } = "";
    [Parameter] public string Class { get; set; } = "size-5";
    [Parameter] public string Stroke { get; set; } = "2";

    private string Body => Name switch
    {
        "dashboard" => "<rect width='7' height='9' x='3' y='3' rx='1'/><rect width='7' height='5' x='14' y='3' rx='1'/><rect width='7' height='9' x='14' y='12' rx='1'/><rect width='7' height='5' x='3' y='16' rx='1'/>",
        "bell" => "<path d='M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9'/><path d='M10.3 21a1.94 1.94 0 0 0 3.4 0'/>",
        "book" => "<path d='M12 7v14'/><path d='M3 18a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1h5a4 4 0 0 1 4 4 4 4 0 0 1 4-4h5a1 1 0 0 1 1 1v13a1 1 0 0 1-1 1h-6a3 3 0 0 0-3 3 3 3 0 0 0-3-3z'/>",
        "library" => "<path d='m16 6 4 14'/><path d='M12 6v14'/><path d='M8 8v12'/><path d='M4 4v16'/>",
        "calendar" => "<path d='M8 2v4'/><path d='M16 2v4'/><rect width='18' height='18' x='3' y='4' rx='2'/><path d='M3 10h18'/><path d='M8 14h.01'/><path d='M12 14h.01'/><path d='M16 14h.01'/><path d='M8 18h.01'/><path d='M12 18h.01'/><path d='M16 18h.01'/>",
        "trophy" => "<path d='M6 9H4.5a2.5 2.5 0 0 1 0-5H6'/><path d='M18 9h1.5a2.5 2.5 0 0 0 0-5H18'/><path d='M4 22h16'/><path d='M10 14.66V17c0 .55-.47.98-.97 1.21C7.85 18.75 7 20.24 7 22'/><path d='M14 14.66V17c0 .55.47.98.97 1.21C16.15 18.75 17 20.24 17 22'/><path d='M18 2H6v7a6 6 0 0 0 12 0V2Z'/>",
        "settings" => "<circle cx='12' cy='12' r='3'/><path d='M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 .6V21a2 2 0 1 1-4 0v-.09A1.65 1.65 0 0 0 8.6 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-.6-1H3a2 2 0 1 1 0-4h.09A1.65 1.65 0 0 0 4.6 8.6a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33h.08A1.65 1.65 0 0 0 10 3.09V3a2 2 0 1 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82v.08a1.65 1.65 0 0 0 1.51 1H21a2 2 0 1 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z'/>",
        "chevron-right" => "<path d='m9 18 6-6-6-6'/>",
        "chevron-left" => "<path d='m15 18-6-6 6-6'/>",
        "x" => "<path d='M18 6 6 18'/><path d='m6 6 12 12'/>",
        "check" => "<path d='M20 6 9 17l-5-5'/>",
        "clock" => "<circle cx='12' cy='12' r='10'/><polyline points='12 6 12 12 16 14'/>",
        "file" => "<path d='M15 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7Z'/><path d='M14 2v4a2 2 0 0 0 2 2h4'/><path d='M16 13H8'/><path d='M16 17H8'/><path d='M10 9H8'/>",
        "chart" => "<path d='M3 3v18h18'/><path d='M18 17V9'/><path d='M13 17V5'/><path d='M8 17v-3'/>",
        "clipboard-check" => "<rect width='8' height='4' x='8' y='2' rx='1'/><path d='M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2'/><path d='m9 14 2 2 4-4'/>",
        "users" => "<path d='M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2'/><circle cx='9' cy='7' r='4'/><path d='M22 21v-2a4 4 0 0 0-3-3.87'/><path d='M16 3.13a4 4 0 0 1 0 7.75'/>",
        "play" => "<polygon points='6 3 20 12 6 21 6 3'/>",
        "pause" => "<rect x='6' y='4' width='4' height='16' rx='1'/><rect x='14' y='4' width='4' height='16' rx='1'/>",
        "plus" => "<path d='M5 12h14'/><path d='M12 5v14'/>",
        "mail" => "<rect width='20' height='16' x='2' y='4' rx='2'/><path d='m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7'/>",
        "lock" => "<circle cx='12' cy='16' r='1'/><rect x='3' y='10' width='18' height='12' rx='2'/><path d='M7 10V7a5 5 0 0 1 10 0v3'/>",
        "eye" => "<path d='M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z'/><circle cx='12' cy='12' r='3'/>",
        "eye-off" => "<path d='M9.88 9.88a3 3 0 1 0 4.24 4.24'/><path d='M10.73 5.08A10.43 10.43 0 0 1 12 5c7 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68'/><path d='M6.61 6.61A13.526 13.526 0 0 0 2 12s3 7 10 7a9.74 9.74 0 0 0 5.39-1.61'/><line x1='2' x2='22' y1='2' y2='22'/>",
        "arrow-left" => "<path d='m12 19-7-7 7-7'/><path d='M19 12H5'/>",
        "edit" => "<path d='M12 20h9'/><path d='M16.5 3.5a2.12 2.12 0 0 1 3 3L7 19l-4 1 1-4Z'/>",
        "award" => "<circle cx='12' cy='8' r='6'/><path d='M15.477 12.89 17 22l-5-3-5 3 1.523-9.11'/>",
        "flame" => "<path d='M8.5 14.5A2.5 2.5 0 0 0 11 12c0-1.38-.5-2-1-3-1.072-2.143-.224-4.054 2-6 .5 2.5 2 4.9 4 6.5 2 1.6 3 3.5 3 5.5a7 7 0 1 1-14 0c0-1.153.433-2.294 1-3a2.5 2.5 0 0 0 2.5 2.5z'/>",
        "shield" => "<path d='M20 13c0 5-3.5 7.5-7.66 8.95a1 1 0 0 1-.67-.01C7.5 20.5 4 18 4 13V6a1 1 0 0 1 1-1c2 0 4.5-1.2 6.24-2.72a1.17 1.17 0 0 1 1.52 0C14.51 3.81 17 5 19 5a1 1 0 0 1 1 1z'/><path d='m9 12 2 2 4-4'/>",
        "sliders" => "<line x1='21' x2='14' y1='4' y2='4'/><line x1='10' x2='3' y1='4' y2='4'/><line x1='21' x2='12' y1='12' y2='12'/><line x1='8' x2='3' y1='12' y2='12'/><line x1='21' x2='16' y1='20' y2='20'/><line x1='12' x2='3' y1='20' y2='20'/><line x1='14' x2='14' y1='2' y2='6'/><line x1='8' x2='8' y1='10' y2='14'/><line x1='16' x2='16' y1='18' y2='22'/>",
        "star" => "<polygon points='12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2'/>",
        _ => "<circle cx='12' cy='12' r='9'/>"
    };
}
