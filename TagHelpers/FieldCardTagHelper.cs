using FootballFieldBooking_New.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace FootballFieldBooking_New.TagHelpers
{
    [HtmlTargetElement("field-card")]
    public class FieldCardTagHelper : TagHelper
    {
        [HtmlAttributeName("field-id")]
        public int FieldId { get; set; }

        [HtmlAttributeName("field-name")]
        public string FieldName { get; set; } = "";

        [HtmlAttributeName("field-type")]
        public string FieldType { get; set; } = "";

        [HtmlAttributeName("location")]
        public string Location { get; set; } = "";

        [HtmlAttributeName("image-url")]
        public string ImageUrl { get; set; } = "";

        [HtmlAttributeName("description")]
        public string Description { get; set; } = "";

        [HtmlAttributeName("min-price")]
        public decimal MinPrice { get; set; }

        [HtmlAttributeName("is-available")]
        public bool IsAvailable { get; set; } = true;

        [HtmlAttributeName("show-booking-button")]
        public bool ShowBookingButton { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "card h-100 shadow-sm hover-card");

            var content = new StringBuilder();

            // Image
            content.Append($@"
                <img src='{ImageUrl}' class='card-img-top' alt='{FieldName}' 
                     style='height: 200px; object-fit: cover;'
                     onerror='this.src=""/images/default-field.jpg""'>
            ");

            // Body
            content.Append("<div class='card-body'>");
            content.Append($"<h5 class='card-title'>{FieldName}</h5>");
            content.Append($"<p class='card-text'>");
            content.Append($"<span class='badge bg-success'>{FieldType}</span>");

            if (!IsAvailable)
            {
                content.Append("<span class='badge bg-danger ms-1'>Tạm ngưng</span>");
            }

            content.Append("</p>");
            content.Append($"<p class='card-text text-muted'><i class='bi bi-geo-alt'></i> {Location}</p>");

            if (!string.IsNullOrEmpty(Description))
            {
                content.Append($"<p class='card-text'>{Description}</p>");
            }

            if (MinPrice > 0)
            {
                content.Append($"<p class='card-text'><small class='text-success'>Giá từ: <strong>{MinPrice:N0} VNĐ/giờ</strong></small></p>");
            }

            content.Append("</div>");

            // Footer
            if (ShowBookingButton)
            {
                content.Append("<div class='card-footer bg-white d-flex gap-2'>");
                content.Append($"<a href='/FootballFields/Details/{FieldId}' class='btn btn-outline-success flex-fill'>");
                content.Append("<i class='bi bi-info-circle'></i> Chi tiết</a>");

                if (IsAvailable)
                {
                    content.Append($"<a href='/Bookings/Create?fieldId={FieldId}' class='btn btn-success flex-fill'>");
                    content.Append("<i class='bi bi-calendar-check'></i> Đặt sân</a>");
                }
                else
                {
                    content.Append("<button class='btn btn-secondary flex-fill' disabled>");
                    content.Append("<i class='bi bi-lock'></i> Không khả dụng</button>");
                }

                content.Append("</div>");
            }

            output.Content.SetHtmlContent(content.ToString());
        }
    }
}