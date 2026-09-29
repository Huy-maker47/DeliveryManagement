using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagement.Models
{
    public class PriceTable
    {
        [Key]
        public int PriceTableId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn hình thức giao hàng")]
        public string DeliveryMethod { get; set; } = string.Empty;
        [Precision(10, 2)]
        [Range(0, 100000, ErrorMessage = "Khối lượng tối thiểu không hợp lệ")]
        public decimal MinWeight { get; set; }

        [Precision(10, 2)]
        [Range(0.01, 100000, ErrorMessage = "Khối lượng tối đa không hợp lệ")]
        public decimal MaxWeight { get; set; }
        public string WeightRange { get; set; } = string.Empty;
        public string DistanceRange { get; set; } = string.Empty;
        [Range(0, 1000000000, ErrorMessage = "Giá không hợp lệ")]
        public decimal Price { get; set; }
    }
}