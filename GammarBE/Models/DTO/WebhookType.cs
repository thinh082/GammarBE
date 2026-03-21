using PayOS.Models.Webhooks;

namespace GammarBE.Models.DTO
{
    public class WebhookType
    {
        public string Code { get; set; }        // Mã lỗi (00 là thành công)
        public string Desc { get; set; }        // Mô tả lỗi
        public bool Success { get; set; }       // Trạng thái thành công hay không
        public WebhookData Data { get; set; }   // Chi tiết dữ liệu thanh toán
        public string Signature { get; set; }   // Chữ ký để kiểm tra tính toàn vẹn
    }
    public class WebhookData
    {
        public long OrderCode { get; set; }             // Mã đơn hàng của bạn
        public int Amount { get; set; }                 // Số tiền thanh toán
        public string Description { get; set; }          // Nội dung chuyển khoản
        public string AccountNumber { get; set; }       // Số tài khoản nhận tiền
        public string Reference { get; set; }           // Mã tham chiếu giao dịch
        public string TransactionDateTime { get; set; } // Thời gian giao dịch (yyyy-mm-dd hh:mm:ss)
        public string Currency { get; set; }            // Đơn vị tiền tệ (thường là VND)
        public string PaymentLinkId { get; set; }       // ID của Link thanh toán
        public string Code { get; set; }                // Mã trạng thái giao dịch
        public string Desc { get; set; }                // Mô tả trạng thái

        // Các thông tin tài khoản đối ứng (nếu có)
        public string CounterAccountBankId { get; set; }
        public string CounterAccountBankName { get; set; }
        public string CounterAccountName { get; set; }
        public string CounterAccountNumber { get; set; }

        // Thông tin tài khoản ảo (nếu dùng)
        public string VirtualAccountName { get; set; }
        public string VirtualAccountNumber { get; set; }
    }
}
