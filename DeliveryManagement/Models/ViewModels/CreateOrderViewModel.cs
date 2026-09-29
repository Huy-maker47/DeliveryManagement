using System.ComponentModel.DataAnnotations;

namespace DeliveryManagement.Models.ViewModels
{
    public class CreateOrderViewModel
    {
        // =========================
        // THÔNG TIN NGƯỜI GỬI
        // =========================

        [Required(ErrorMessage = "Vui lòng nhập họ tên người gửi.")]
        [StringLength(100)]
        public string SenderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại người gửi.")]
        [RegularExpression(
            @"^0\d{9}$",
            ErrorMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.")]
        public string SenderPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ người gửi.")]
        [StringLength(255)]
        public string SenderAddress { get; set; } = string.Empty;


        // =========================
        // THÔNG TIN NGƯỜI NHẬN
        // =========================

        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận.")]
        [StringLength(100)]
        public string ReceiverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại người nhận")]
        [RegularExpression(
            @"^0\d{9}$",
            ErrorMessage = "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0")]
        public string ReceiverPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ người nhận")]
        [StringLength(255)]
        public string ReceiverAddress { get; set; } = string.Empty;


        // =========================
        // THÔNG TIN GIAO HÀNG
        // =========================

        [Required(ErrorMessage = "Vui lòng chọn hình thức giao hàng")]
        public string DeliveryMethod { get; set; } = string.Empty;


        // =========================
        // THÔNG TIN THANH TOÁN
        // =========================

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string PaymentMethod { get; set; } = string.Empty;


        // =========================
        // THÔNG TIN HÀNG HÓA (nhiều dòng)
        // =========================

        [Required(ErrorMessage = "Vui lòng thêm ít nhất một hàng hóa")]
        [MinLength(1, ErrorMessage = "Vui lòng thêm ít nhất một hàng hóa")]
        [MaxLength(20, ErrorMessage = "Mỗi đơn tối đa 20 hàng hóa")]
        public List<OrderItemInputModel> Items { get; set; } = new();
    }

    public class OrderItemInputModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn loại đóng gói")]
        public int PackageTypeId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả hàng hóa")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Size { get; set; }

        [Range(1, 1000, ErrorMessage = "Số lượng phải từ 1 đến 1000")]
        public int Quantity { get; set; } = 1;

        [Range(0.01, 10000, ErrorMessage = "Khối lượng phải lớn hơn 0")]
        public decimal Weight { get; set; }

        public bool IsFragile { get; set; }

        public bool IsValuable { get; set; }
    }
}