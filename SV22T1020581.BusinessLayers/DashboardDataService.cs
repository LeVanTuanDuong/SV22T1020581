using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020581.DataLayers.SQLServer;
using SV22T1020581.Models.Sales;

namespace SV22T1020581.BusinessLayers;

/// <summary>
/// Cung cấp các chức năng xử lý dữ liệu cho Dashboard (trang chủ quản trị).
/// </summary>
public static class DashboardDataService
{
    private static readonly string _connectionString;

    static DashboardDataService()
    {
        _connectionString = Configuration.ConnectionString;
    }

    /// <summary>
    /// Lấy doanh thu trong ngày hôm nay.
    /// </summary>
    public static async Task<decimal> GetTodayRevenueAsync()
    {
        await using var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
        var today = DateTime.Now.Date;
        var result = await conn.ExecuteScalarAsync<decimal?>(
            @"SELECT ISNULL(SUM(od.Quantity * od.SalePrice), 0)
              FROM Orders o
              INNER JOIN OrderDetails od ON o.OrderID = od.OrderID
              WHERE CAST(o.OrderTime AS DATE) = @Today
                AND o.Status = @CompletedStatus",
            new { Today = today, CompletedStatus = (int)OrderStatusEnum.Completed });
        return result ?? 0;
    }

    /// <summary>
    /// Lấy tổng số đơn hàng.
    /// </summary>
    public static async Task<int> GetOrderCountAsync()
    {
        await using var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
        return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Orders");
    }

    /// <summary>
    /// Lấy tổng số khách hàng.
    /// </summary>
    public static async Task<int> GetCustomerCountAsync()
    {
        await using var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
        return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Customers");
    }

    /// <summary>
    /// Lấy tổng số mặt hàng.
    /// </summary>
    public static async Task<int> GetProductCountAsync()
    {
        await using var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
        return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Products");
    }

    /// <summary>
    /// Lấy danh sách các đơn hàng đang xử lý (trạng thái: Mới hoặc Đã duyệt hoặc Đang giao).
    /// </summary>
    public static async Task<List<OrderViewInfo>> ListProcessingOrdersAsync()
    {
        await using var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
        var sql = @"
            SELECT o.OrderID, o.CustomerID,
                c.CustomerName,
                o.OrderTime, o.DeliveryProvince, o.DeliveryAddress,
                o.EmployeeID, CAST(e.FullName AS NVARCHAR(200)) AS EmployeeName,
                o.ShipperID, s.ShipperName,
                o.Status, os.Description AS StatusDescription,
                ISNULL((
                    SELECT SUM(CAST(od.Quantity AS decimal(18,4)) * CAST(od.SalePrice AS decimal(18,4)))
                    FROM OrderDetails od WHERE od.OrderID = o.OrderID
                ), 0) AS DetailsTotalValue
            FROM Orders o
            LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
            LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
            LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
            INNER JOIN OrderStatus os ON o.Status = os.Status
            WHERE o.Status IN (@New, @Accepted, @Shipping)
            ORDER BY o.OrderTime DESC";
        var rows = await conn.QueryAsync<OrderViewInfo>(sql,
            new
            {
                New = (int)OrderStatusEnum.New,
                Accepted = (int)OrderStatusEnum.Accepted,
                Shipping = (int)OrderStatusEnum.Shipping
            });
        return rows.ToList();
    }
}
