namespace SV22T1020581.Models.Sales
{
    /// <summary>
    /// Thong tin day du cua mot don hang (DTO).
    /// </summary>
    public class OrderViewInfo : Order
    {
        /// <summary>Ten nhan vien phu trach don hang.</summary>
        public string EmployeeName { get; set; } = "";
        /// <summary>Ten khach hang.</summary>
        public string CustomerName { get; set; } = "";
        /// <summary>Ten giao dich cua khach hang.</summary>
        public string CustomerContactName { get; set; } = "";
        /// <summary>Email cua khach hang.</summary>
        public string CustomerEmail { get; set; } = "";
        /// <summary>Dien thoai khach hang.</summary>
        public string CustomerPhone { get; set; } = "";
        /// <summary>Dia chi cua khach hang.</summary>
        public string CustomerAddress { get; set; } = "";
        /// <summary>Ten nguoi giao hang.</summary>
        public string ShipperName { get; set; } = "";
        /// <summary>Dien thoai nguoi giao hang.</summary>
        public string ShipperPhone { get; set; } = "";
        /// <summary>Mo ta trang thai don hang.</summary>
        public string StatusDescription => ((OrderStatusEnum)Status).GetDescription();
        /// <summary>Tong gia tri don hang dua tren chi tiet mat hang.</summary>
        public decimal DetailsTotalValue { get; set; }
        /// <summary>Tong gia tri don hang.</summary>
        public decimal TotalValue => DetailsTotalValue;
        /// <summary>Tong gia tri don hang (alias dung boi View).</summary>
        public decimal TotalAmount => DetailsTotalValue;
    }
}
