using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FootballFieldBooking_New.TagHelpers
{
    [HtmlTargetElement("alert")]
    public class AlertTagHelper : TagHelper
    {
        [HtmlAttributeName("type")]
        public string Type { get; set; } = "info"; // success, danger, warning, info

        [HtmlAttributeName("icon")]
        public string Icon { get; set; } = "";

        [HtmlAttributeName("dismissible")]
        public bool Dismissible { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";

            string alertClass = $"alert alert-{Type}";
            if (Dismissible)
            {
                alertClass += " alert-dismissible fade show";
            }

            output.Attributes.SetAttribute("class", alertClass);
            output.Attributes.SetAttribute("role", "alert");

            // Auto icon based on type
            if (string.IsNullOrEmpty(Icon))
            {
                Icon = Type switch
                {
                    "success" => "bi-check-circle",
                    "danger" => "bi-x-circle",
                    "warning" => "bi-exclamation-triangle",
                    "info" => "bi-info-circle",
                    _ => "bi-info-circle"
                };
            }

            var content = $"<i class='bi {Icon}'></i> ";
            output.PreContent.SetHtmlContent(content);

            if (Dismissible)
            {
                output.PostContent.SetHtmlContent("<button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>");
            }
        }
    }
}