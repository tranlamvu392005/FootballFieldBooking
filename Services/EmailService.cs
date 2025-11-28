using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace FootballFieldBooking_New.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        // ===== Gửi email xác nhận đặt sân =====
        public async Task SendBookingConfirmationAsync(
            string toEmail,
            string userName,
            int bookingId,
            string fieldName,
            DateTime playDate,
            string timeSlot,
            decimal totalPrice)
        {
            var subject = $"✅ Đặt sân thành công - Mã đơn #{bookingId}";

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                   color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .info-box {{ background: white; padding: 20px; margin: 20px 0; border-radius: 8px; 
                     border-left: 4px solid #28a745; }}
        .info-row {{ display: flex; justify-content: space-between; padding: 10px 0; 
                     border-bottom: 1px solid #eee; }}
        .label {{ font-weight: bold; color: #666; }}
        .value {{ color: #333; }}
        .price {{ font-size: 24px; color: #28a745; font-weight: bold; }}
        .button {{ background: #28a745; color: white; padding: 12px 30px; 
                   text-decoration: none; border-radius: 5px; display: inline-block; 
                   margin: 20px 0; }}
        .footer {{ text-align: center; color: #666; margin-top: 30px; font-size: 12px; }}
        .warning {{ background: #fff3cd; padding: 15px; border-radius: 5px; 
                    border-left: 4px solid #ffc107; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Đặt sân thành công!</h1>
            <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi</p>
        </div>
        
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            <p>Đơn đặt sân của bạn đã được xác nhận thành công. Dưới đây là thông tin chi tiết:</p>
            
            <div class='info-box'>
                <div class='info-row'>
                    <span class='label'>📋 Mã đơn:</span>
                    <span class='value'>#{bookingId}</span>
                </div>
                <div class='info-row'>
                    <span class='label'>⚽ Sân bóng:</span>
                    <span class='value'>{fieldName}</span>
                </div>
                <div class='info-row'>
                    <span class='label'>📅 Ngày chơi:</span>
                    <span class='value'>{playDate:dd/MM/yyyy (dddd)}</span>
                </div>
                <div class='info-row'>
                    <span class='label'>🕐 Giờ chơi:</span>
                    <span class='value'>{timeSlot}</span>
                </div>
                <div class='info-row' style='border-bottom: none;'>
                    <span class='label'>💰 Tổng tiền:</span>
                    <span class='price'>{totalPrice:N0} VNĐ</span>
                </div>
            </div>
            
            <div class='warning'>
                <strong>⚠️ Lưu ý quan trọng:</strong>
                <ul style='margin: 10px 0; padding-left: 20px;'>
                    <li>Vui lòng đến đúng giờ để tận dụng thời gian chơi</li>
                    <li>Thanh toán trực tiếp tại sân khi đến chơi</li>
                    <li>Hủy đơn trước 24h nếu có thay đổi kế hoạch</li>
                    <li>Liên hệ hotline: <strong>1900-xxxx</strong> nếu cần hỗ trợ</li>
                </ul>
            </div>
            
            <div style='text-align: center;'>
                <a href='https://localhost:7186/Bookings/Details/{bookingId}' class='button'>
                    Xem chi tiết đơn đặt
                </a>
            </div>
            
            <p style='margin-top: 30px;'>Chúc bạn có trận đấu vui vẻ! ⚽🔥</p>
        </div>
        
        <div class='footer'>
            <p>Email này được gửi tự động từ hệ thống đặt sân bóng</p>
            <p>© 2024 Football Field Booking. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        // ===== Gửi email hủy đặt sân =====
        public async Task SendBookingCancellationAsync(string toEmail, string userName, int bookingId)
        {
            var subject = $"❌ Đơn đặt sân #{bookingId} đã bị hủy";

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #dc3545; color: white; padding: 30px; text-align: center; 
                   border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ background: #28a745; color: white; padding: 12px 30px; 
                   text-decoration: none; border-radius: 5px; display: inline-block; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Đơn đặt sân đã bị hủy</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            <p>Đơn đặt sân <strong>#{bookingId}</strong> của bạn đã bị hủy.</p>
            <p>Nếu bạn không thực hiện thao tác này, vui lòng liên hệ với chúng tôi ngay.</p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='https://localhost:7186/FootballFields' class='button'>
                    Đặt sân mới
                </a>
            </div>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        // ===== Gửi email nhắc nhở =====
        public async Task SendBookingReminderAsync(
            string toEmail,
            string userName,
            string fieldName,
            DateTime playDate,
            string timeSlot)
        {
            var subject = $"⏰ Nhắc nhở: Bạn có lịch chơi vào {playDate:dd/MM/yyyy}";

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #ffc107; color: #333; padding: 30px; text-align: center; 
                   border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⏰ Nhắc nhở lịch chơi</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            <p>Đây là email nhắc nhở về lịch đặt sân của bạn:</p>
            <ul>
                <li><strong>Sân:</strong> {fieldName}</li>
                <li><strong>Ngày:</strong> {playDate:dd/MM/yyyy (dddd)}</li>
                <li><strong>Giờ:</strong> {timeSlot}</li>
            </ul>
            <p>Vui lòng đến đúng giờ. Chúc bạn có trận đấu vui vẻ!</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        // ===== Core method gửi email =====
        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = htmlBody;
                message.IsBodyHtml = true;

                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                smtpClient.Credentials = new NetworkCredential(
                    _emailSettings.SenderEmail,
                    _emailSettings.SenderPassword
                );
                smtpClient.EnableSsl = _emailSettings.EnableSsl;

                await smtpClient.SendMailAsync(message);

                _logger.LogInformation($"✅ Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Failed to send email to {toEmail}");
                // Không throw exception để không làm crash booking process
            }
        }
    }
}
