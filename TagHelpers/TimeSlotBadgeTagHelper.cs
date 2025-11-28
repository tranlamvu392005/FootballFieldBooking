using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FootballFieldBooking_New.TagHelpers
{
    [HtmlTargetElement("timeslot-badge")]
    public class TimeSlotBadgeTagHelper : TagHelper
    {
        [HtmlAttributeName("start-time")]
        public TimeSpan StartTime { get; set; }

        [HtmlAttributeName("end-time")]
        public TimeSpan EndTime { get; set; }

        [HtmlAttributeName("is-weekend")]
        public bool IsWeekend { get; set; }

        [HtmlAttributeName("is-booked")]
        public bool IsBooked { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";

            string badgeClass = "badge ";

            if (IsBooked)
            {
                badgeClass += "bg-secondary text-decoration-line-through";
            }
            else if (IsWeekend)
            {
                badgeClass += "bg-warning text-dark";
            }
            else
            {
                badgeClass += "bg-info text-dark";
            }

            output.Attributes.SetAttribute("class", badgeClass);

            string status = IsBooked ? " (Đã đặt)" : "";
            output.Content.SetHtmlContent($"<i class='bi bi-clock'></i> {StartTime:hh\\:mm} - {EndTime:hh\\:mm}{status}");
        }
    }
}