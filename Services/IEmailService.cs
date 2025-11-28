namespace FootballFieldBooking_New.Services
{
    /// <summary>
    /// Interface định nghĩa các phương thức gửi email
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Gửi email xác nhận khi đặt sân thành công
        /// </summary>
        /// <param name="toEmail">Email người nhận</param>
        /// <param name="userName">Tên khách hàng</param>
        /// <param name="bookingId">Mã đơn đặt</param>
        /// <param name="fieldName">Tên sân</param>
        /// <param name="playDate">Ngày chơi</param>
        /// <param name="timeSlot">Khung giờ (VD: "18:00 - 20:00")</param>
        /// <param name="totalPrice">Tổng tiền</param>
        Task SendBookingConfirmationAsync(
            string toEmail,
            string userName,
            int bookingId,
            string fieldName,
            DateTime playDate,
            string timeSlot,
            decimal totalPrice);

        /// <summary>
        /// Gửi email thông báo khi hủy đơn đặt sân
        /// </summary>
        /// <param name="toEmail">Email người nhận</param>
        /// <param name="userName">Tên khách hàng</param>
        /// <param name="bookingId">Mã đơn đặt</param>
        Task SendBookingCancellationAsync(
            string toEmail,
            string userName,
            int bookingId);

        /// <summary>
        /// Gửi email nhắc nhở trước giờ chơi (dùng cho scheduled job sau này)
        /// </summary>
        /// <param name="toEmail">Email người nhận</param>
        /// <param name="userName">Tên khách hàng</param>
        /// <param name="fieldName">Tên sân</param>
        /// <param name="playDate">Ngày chơi</param>
        /// <param name="timeSlot">Khung giờ</param>
        Task SendBookingReminderAsync(
            string toEmail,
            string userName,
            string fieldName,
            DateTime playDate,
            string timeSlot);
    }
}