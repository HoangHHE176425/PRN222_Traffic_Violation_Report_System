namespace Traffic_Violation_Reporting_Management_System.Models
{
    public class PayOSWebhookModel
    {
        public long OrderCode { get; set; }           // Mã đơn hàng bạn tạo (fineId)
        public int Amount { get; set; }               // Số tiền thanh toán
        public string Description { get; set; } = ""; // Mô tả
        public string Status { get; set; } = "";      // Trạng thái thanh toán, ví dụ: "PAID"
        public string TransactionId { get; set; } = ""; // Mã giao dịch do PayOS cấp
        public long Time { get; set; }                // Thời gian giao dịch (epoch milliseconds)
        public string Signature { get; set; } = "";   // Chữ ký dùng để kiểm tra tính hợp lệ
    }
}
