using FootballFieldBooking_New.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FootballFieldBooking_New.TagHelpers
{
    [HtmlTargetElement("status-badge")]
    public class StatusBadgeTagHelper : TagHelper
    {
        [HtmlAttributeName("status")]
        public BookingStatus Status { get; set; }

        [HtmlAttributeName("show-icon")]
        public bool ShowIcon { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";

            string badgeClass = "";
            string icon = "";
            string text = "";

            switch (Status)
            {
                case BookingStatus.Pending:
                    badgeClass = "badge bg-warning text-dark";
                    icon = "bi-clock-history";
                    text = "Chờ xác nhận";
                    break;
                case BookingStatus.Confirmed:
                    badgeClass = "badge bg-success";
                    icon = "bi-check-circle";
                    text = "Đã xác nhận";
                    break;
                case BookingStatus.Cancelled:
                    badgeClass = "badge bg-danger";
                    icon = "bi-x-circle";
                    text = "Đã hủy";
                    break;
                case BookingStatus.Completed:
                    badgeClass = "badge bg-info";
                    icon = "bi-check-all";
                    text = "Hoàn thành";
                    break;
            }

            output.Attributes.SetAttribute("class", badgeClass);

            if (ShowIcon)
            {
                output.Content.SetHtmlContent($"<i class='bi {icon}'></i> {text}");
            }
            else
            {
                output.Content.SetContent(text);
            }
        }
    }
}