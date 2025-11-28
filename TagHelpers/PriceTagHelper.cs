using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FootballFieldBooking_New.TagHelpers
{
    [HtmlTargetElement("price")]
    public class PriceTagHelper : TagHelper
    {
        [HtmlAttributeName("amount")]
        public decimal Amount { get; set; }

        [HtmlAttributeName("currency")]
        public string Currency { get; set; } = "VNĐ";

        [HtmlAttributeName("color")]
        public string Color { get; set; } = "success";

        [HtmlAttributeName("size")]
        public string Size { get; set; } = "normal"; // normal, large, small

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";

            string sizeClass = Size switch
            {
                "large" => "fs-4",
                "small" => "small",
                _ => ""
            };

            output.Attributes.SetAttribute("class", $"text-{Color} fw-bold {sizeClass}");
            output.Content.SetContent($"{Amount:N0} {Currency}");
        }
    }
}