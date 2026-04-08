using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020581.DataLayers;
using SV22T1020581.DataLayers.Interfaces;
using SV22T1020581.Models.Common;
using SV22T1020581.Models.Sales;

namespace SV22T1020581.DataLayers.SQLServer
{
    /// <summary>
    /// Truy van Orders va OrderDetails.
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        /// <summary>Khoi tao voi chuoi ket noi SQL Server.</summary>
        public OrderRepository(string connectionString) => _connectionString = connectionString;

        private const string OrderSelect = @"
            SELECT o.OrderID, o.CustomerID, c.CustomerName, c.ContactName AS CustomerContactName,
                c.Phone AS CustomerPhone,
                o.OrderTime, o.DeliveryProvince, o.DeliveryAddress,
                o.EmployeeID, CAST(e.FullName AS NVARCHAR(200)) AS EmployeeName, o.AcceptTime,
                o.ShipperID, s.ShipperName, o.ShippedTime, o.FinishedTime,
                o.Status, os.Description AS StatusDescription,
                ISNULL((
                    SELECT SUM(CAST(od.Quantity AS decimal(18,4)) * CAST(od.SalePrice AS decimal(18,4)))
                    FROM OrderDetails od WHERE od.OrderID = o.OrderID
                ), 0) AS DetailsTotalValue
            FROM Orders o
            LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
            LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
            LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
            INNER JOIN OrderStatus os ON o.Status = os.Status";

        /// <inheritdoc />
        public async Task<PagedResult<OrderViewInfo>> ListAsync(SV22T1020581.Models.Sales.OrderSearchInput input)
        {
            await using var conn = new SqlConnection(_connectionString);
            var like = SqlLike.Pattern(input.SearchValue);
            // Giao diện dùng "-- Tất cả --" => Status = 0; trong CSDL không có trạng thái 0, phải bỏ lọc theo trạng thái.
            int? statusFilter = (int)input.Status == 0 ? null : (int)input.Status;

            var where = @"WHERE (@Search = '' OR CAST(o.OrderID AS nvarchar(20)) LIKE @Like OR c.CustomerName LIKE @Like OR ISNULL(o.DeliveryAddress,'') LIKE @Like)
                AND (@Status IS NULL OR o.Status = @Status)
                AND (@DateFrom IS NULL OR CAST(o.OrderTime AS DATE) >= @DateFrom)
                AND (@DateTo IS NULL OR CAST(o.OrderTime AS DATE) <= @DateTo)";
            var countSql = $@"
                SELECT COUNT(*) FROM Orders o
                LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                {where}";
            var rowCount = await conn.ExecuteScalarAsync<int>(countSql,
                new { Search = input.SearchValue ?? "", Like = like, Status = statusFilter, input.DateFrom, input.DateTo });
            var result = new PagedResult<OrderViewInfo> { Page = input.Page, PageSize = input.PageSize, RowCount = rowCount };
            if (input.PageSize == 0)
            {
                var all = await conn.QueryAsync<OrderViewInfo>($@"
                    {OrderSelect} {where} ORDER BY o.OrderTime DESC",
                    new { Search = input.SearchValue ?? "", Like = like, Status = statusFilter, input.DateFrom, input.DateTo });
                result.DataItems = all.ToList();
                return result;
            }
            var data = await conn.QueryAsync<OrderViewInfo>($@"
                SELECT OrderID, CustomerID, CustomerName, CustomerContactName, CustomerPhone, OrderTime, DeliveryProvince, DeliveryAddress,
                    EmployeeID, EmployeeName, AcceptTime, ShipperID, ShipperName, ShippedTime, FinishedTime, Status, StatusDescription, DetailsTotalValue
                FROM (
                    SELECT ROW_NUMBER() OVER (ORDER BY o.OrderTime DESC) AS rn,
                        o.OrderID, o.CustomerID, c.CustomerName, c.ContactName AS CustomerContactName,
                        c.Phone AS CustomerPhone,
                        o.OrderTime, o.DeliveryProvince, o.DeliveryAddress,
                        o.EmployeeID, CAST(e.FullName AS NVARCHAR(200)) AS EmployeeName, o.AcceptTime,
                        o.ShipperID, s.ShipperName, o.ShippedTime, o.FinishedTime,
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
                    {where}
                ) x WHERE rn > @Offset AND rn <= @Offset + @Fetch",
                new { Search = input.SearchValue ?? "", Like = like, Status = statusFilter, input.DateFrom, input.DateTo, Offset = input.Offset, Fetch = input.PageSize });
            result.DataItems = data.ToList();
            return result;
        }

        /// <inheritdoc />
        public async Task<OrderViewInfo?> GetAsync(int orderId)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<OrderViewInfo>($@"
                {OrderSelect} WHERE o.OrderID=@orderId", new { orderId });
        }

        /// <inheritdoc />
        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderId)
        {
            await using var conn = new SqlConnection(_connectionString);
            var rows = await conn.QueryAsync<OrderDetailViewInfo>(@"
                SELECT od.OrderID, od.ProductID, p.ProductName, p.Unit, od.Quantity, od.SalePrice
                FROM OrderDetails od
                INNER JOIN Products p ON od.ProductID = p.ProductID
                WHERE od.OrderID=@orderId
                ORDER BY p.ProductName", new { orderId });
            return rows.ToList();
        }

        /// <inheritdoc />
        public async Task<OrderDetailViewInfo?> GetDetailLineAsync(int orderId, int productId)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<OrderDetailViewInfo>(@"
                SELECT od.OrderID, od.ProductID, p.ProductName, p.Unit, od.Quantity, od.SalePrice
                FROM OrderDetails od
                INNER JOIN Products p ON od.ProductID = p.ProductID
                WHERE od.OrderID=@orderId AND od.ProductID=@productId", new { orderId, productId });
        }

        /// <inheritdoc />
        public async Task<int> AddAsync(Order order)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Orders (CustomerID, OrderTime, DeliveryProvince, DeliveryAddress, EmployeeID, AcceptTime, ShipperID, ShippedTime, FinishedTime, Status)
                VALUES (@CustomerID, @OrderTime, @DeliveryProvince, @DeliveryAddress, @EmployeeID, @AcceptTime, @ShipperID, @ShippedTime, @FinishedTime, @Status);
                SELECT CAST(SCOPE_IDENTITY() AS int);", order);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Order order)
        {
            await using var conn = new SqlConnection(_connectionString);
            var n = await conn.ExecuteAsync(@"
                UPDATE Orders SET CustomerID=@CustomerID, OrderTime=@OrderTime, DeliveryProvince=@DeliveryProvince,
                    DeliveryAddress=@DeliveryAddress, EmployeeID=@EmployeeID, AcceptTime=@AcceptTime,
                    ShipperID=@ShipperID, ShippedTime=@ShippedTime, FinishedTime=@FinishedTime, Status=@Status
                WHERE OrderID=@OrderID", order);
            return n > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int orderId)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync("DELETE FROM OrderDetails WHERE OrderID=@orderId", new { orderId });
            return await conn.ExecuteAsync("DELETE FROM Orders WHERE OrderID=@orderId", new { orderId }) > 0;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateStatusAsync(int orderID, OrderStatusEnum status, DateTime? acceptTime, DateTime? finishedTime, int? employeeID)
        {
            await using var conn = new SqlConnection(_connectionString);
            var n = await conn.ExecuteAsync(@"
                UPDATE Orders SET Status=@status, AcceptTime=@acceptTime,
                    FinishedTime=@finishedTime, EmployeeID=@employeeID
                WHERE OrderID=@orderID",
                new { orderID, status, acceptTime, finishedTime, employeeID });
            return n > 0;
        }

        /// <inheritdoc />
        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.QueryFirstOrDefaultAsync<OrderDetailViewInfo>(@"
                SELECT od.OrderID, od.ProductID, p.ProductName, p.Unit, od.Quantity, od.SalePrice
                FROM OrderDetails od
                INNER JOIN Products p ON od.ProductID = p.ProductID
                WHERE od.OrderID=@orderID AND od.ProductID=@productID",
                new { orderID, productID });
        }

        /// <inheritdoc />
        public async Task<bool> AddDetailAsync(OrderDetail detail)
        {
            await using var conn = new SqlConnection(_connectionString);
            var exists = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM OrderDetails WHERE OrderID=@OrderID AND ProductID=@ProductID", detail);
            if (exists > 0) return false;
            var n = await conn.ExecuteAsync(@"
                INSERT INTO OrderDetails (OrderID, ProductID, Quantity, SalePrice)
                VALUES (@OrderID, @ProductID, @Quantity, @SalePrice)", detail);
            return n > 0;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateDetailAsync(int orderId, int productId, int quantity, decimal salePrice)
        {
            await using var conn = new SqlConnection(_connectionString);
            var n = await conn.ExecuteAsync(@"
                UPDATE OrderDetails SET Quantity=@quantity, SalePrice=@salePrice
                WHERE OrderID=@orderId AND ProductID=@productId",
                new { orderId, productId, quantity, salePrice });
            return n > 0;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteDetailAsync(int orderId, int productId)
        {
            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteAsync(
                "DELETE FROM OrderDetails WHERE OrderID=@orderId AND ProductID=@productId", new { orderId, productId }) > 0;
        }

        /// <inheritdoc />
        public async Task<PagedResult<OrderViewInfo>> GetByCustomerAsync(SV22T1020581.Models.Common.OrderSearchInput input)
        {
            await using var conn = new SqlConnection(_connectionString);
            var like = SqlLike.Pattern(input.SearchValue ?? "");
            var where = @"WHERE (@Search = '' OR CAST(o.OrderID AS nvarchar(20)) LIKE @Like)
                AND (@CustomerID IS NULL OR o.CustomerID = @CustomerID)";

            var rowCount = await conn.ExecuteScalarAsync<int>($@"
                SELECT COUNT(*) FROM Orders o
                LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                {where}",
                new { Search = input.SearchValue ?? "", Like = like, input.CustomerID });

            var result = new PagedResult<OrderViewInfo> { Page = input.Page, PageSize = input.PageSize, RowCount = rowCount };

            if (input.PageSize == 0)
            {
                var all = await conn.QueryAsync<OrderViewInfo>($@"
                    {OrderSelect} {where} ORDER BY o.OrderTime DESC",
                    new { Search = input.SearchValue ?? "", Like = like, input.CustomerID });
                result.DataItems = all.ToList();
                return result;
            }

            var data = await conn.QueryAsync<OrderViewInfo>($@"
                SELECT OrderID, CustomerID, CustomerName, CustomerContactName, CustomerPhone, OrderTime, DeliveryProvince, DeliveryAddress,
                    EmployeeID, EmployeeName, AcceptTime, ShipperID, ShipperName, ShippedTime, FinishedTime, Status, StatusDescription, DetailsTotalValue
                FROM (
                    SELECT ROW_NUMBER() OVER (ORDER BY o.OrderTime DESC) AS rn,
                        o.OrderID, o.CustomerID, c.CustomerName, c.ContactName AS CustomerContactName,
                        c.Phone AS CustomerPhone,
                        o.OrderTime, o.DeliveryProvince, o.DeliveryAddress,
                        o.EmployeeID, CAST(e.FullName AS NVARCHAR(200)) AS EmployeeName, o.AcceptTime,
                        o.ShipperID, s.ShipperName, o.ShippedTime, o.FinishedTime,
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
                    {where}
                ) x WHERE rn > @Offset AND rn <= @Offset + @Fetch",
                new { Search = input.SearchValue ?? "", Like = like, input.CustomerID, Offset = input.Offset, Fetch = input.PageSize });

            result.DataItems = data.ToList();
            return result;
        }
    }
}
