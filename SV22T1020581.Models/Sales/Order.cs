namespace SV22T1020581.Models.Sales
{
    /// <summary>
    /// Don hang.
    /// </summary>
    public class Order
    {
        /// <summary>Ma don hang.</summary>
        public int OrderID { get; set; }
        /// <summary>Ma khach hang.</summary>
        public int? CustomerID { get; set; }
        /// <summary>Thoi diem dat hang.</summary>
        public DateTime OrderTime { get; set; }
        /// <summary>Tinh/thanh giao hang.</summary>
        public string? DeliveryProvince { get; set; }
        /// <summary>Dia chi giao hang.</summary>
        public string? DeliveryAddress { get; set; }
        /// <summary>Ma nhan vien xu ly don hang.</summary>
        public int? EmployeeID { get; set; }
        /// <summary>Thoi diem duyet don hang.</summary>
        public DateTime? AcceptTime { get; set; }
        /// <summary>Ma nguoi giao hang.</summary>
        public int? ShipperID { get; set; }
        /// <summary>Thoi diem nguoi giao hang nhan don.</summary>
        public DateTime? ShippedTime { get; set; }
        /// <summary>Thoi diem ket thuc don hang.</summary>
        public DateTime? FinishedTime { get; set; }
        /// <summary>
        /// Trang thai hien tai cua don hang (gia tri int, dung voi cot Status trong CSDL).
        /// Dung OrderStatusEnum de kiem tra: (OrderStatusEnum)Status == OrderStatusEnum.New
        /// </summary>
        public int Status { get; set; }
    }
}
