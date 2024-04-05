using SV20T1020025.DomainModels;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using Azure;

namespace SV20T1020025.DataLayers.SQLServer
{
    public class OrderDAL : _BaseDAL, IOrderDAL
    {
        public OrderDAL(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Chuyển dữ liệu từ SqlDataReader thành Order
        /// </summary>
        /// <param name="dbReader"></param>
        /// <returns></returns>
        private Order DataReaderToOrder(SqlDataReader dbReader)
        {
            return new Order()
            {
                OrderID = Convert.ToInt32(dbReader["OrderID"]),
                OrderTime = Convert.ToDateTime(dbReader["OrderTime"]),
                AcceptTime = DBValueToNullableDateTime(dbReader["AcceptTime"]),
                ShippedTime = DBValueToNullableDateTime(dbReader["ShippedTime"]),
                FinishedTime = DBValueToNullableDateTime(dbReader["FinishedTime"]),
                Status = Convert.ToInt32(dbReader["Status"]),
                CustomerID = (int)DBValueToNullableInt(dbReader["CustomerID"]),
                CustomerName = dbReader["CustomerName"].ToString(),
                CustomerContactName = dbReader["CustomerContactName"].ToString(),
                CustomerAddress = dbReader["CustomerAddress"].ToString(),
                CustomerEmail = dbReader["CustomerEmail"].ToString(),

                EmployeeID = DBValueToNullableInt(dbReader["EmployeeID"]),
                EmployeeName = dbReader["EmployeeName"].ToString(),

                ShipperID = DBValueToNullableInt(dbReader["ShipperID"]),
                ShipperName = dbReader["ShipperName"].ToString(),
                ShipperPhone = dbReader["ShipperPhone"].ToString()
            };
        }


        public int Add(Order data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"insert into Orders(CustomerId, OrderTime,DeliveryProvince, DeliveryAddress,EmployeeID, Status)
                    values(@CustomerID, getdate(),@DeliveryProvince, @DeliveryAddress,@EmployeeID, @Status);
                    select @@identity";
                var orderParameters = new
                {
                    CustomerID = data.CustomerID,
                    DeliveryProvince = data.DeliveryProvince,
                    DeliveryAddress = data.DeliveryAddress,
                    EmployeeID = data.EmployeeID,
                    Status = data.Status
                };
                id = connection.ExecuteScalar<int>(sql: sql, param: orderParameters, commandType: CommandType.Text);
                connection.Close();
            }
            return id;
        }

        public int Count(int status = 0, DateTime? fromTime = null, DateTime? toTime = null, string searchValue = "")
        {
            int count = 0;
            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";
            using (var connection = OpenConnection())
            {
                var sql = @"select count(*)
                            from Orders as o
                            left join Customers as c on o.CustomerID = c.CustomerID
                            left join Employees as e on o.EmployeeID = e.EmployeeID
                            left join Shippers as s on o.ShipperID = s.ShipperID

                            where (@Status = 0 or o.Status = @Status)
                            and (@FromTime is null or o.OrderTime >= @FromTime)
                            and (@ToTime is null or o.OrderTime <= @ToTime)
                            and (@SearchValue = N''
                            or c.CustomerName like @SearchValue
                            or e.FullName like @SearchValue
                            or s.ShipperName like @SearchValue)";
                var parameters = new
                {
                    Status = status,
                    FromTime = fromTime,
                    ToTime = toTime,
                    SearchValue = searchValue ?? ""
                };
                count = connection.ExecuteScalar<int>(sql: sql, param: parameters,
                                                    commandType: CommandType.Text);
                connection.Close();
            }
            return count;
        }

        public bool Delete(int orderID)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM OrderDetails WHERE OrderID = @OrderID;
                            DELETE FROM Orders WHERE OrderID = @OrderID";

                var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@OrderID", orderID);

                result = command.ExecuteNonQuery() > 0;
            }
            return result;
        }

        public bool DeleteDetail(int orderID, int productID)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM OrderDetails WHERE OrderID = @OrderID AND ProductID = @ProductID";

                var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@OrderID", orderID);
                command.Parameters.AddWithValue("@ProductID", productID);

                result = command.ExecuteNonQuery() > 0;
            }
            return result;
        }

        public Order? Get(int orderID)
        {
            Order? data = null;
            using (var connection = OpenConnection())
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"SELECT  o.*,
                                    c.CustomerName,
                                    c.ContactName as CustomerContactName,
                                    c.Address as CustomerAddress,
                                    c.Phone as CustomerPhone,
                                    c.Email as CustomerEmail,
                                    e.FullName as EmployeeName,
                                    s.ShipperName,
                                    s.Phone as ShipperPhone
                            FROM    Orders as o
                                    LEFT JOIN Customers AS c ON o.CustomerID = c.CustomerID
                                    LEFT JOIN Employees AS e ON o.EmployeeID = e.EmployeeID
                                    LEFT JOIN Shippers AS s ON o.ShipperID = s.ShipperID
                            WHERE   o.OrderID = @OrderID";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@OrderID", orderID);

                using (var dbReader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (dbReader.Read())
                    {
                        data = DataReaderToOrder(dbReader);
                    }
                    dbReader.Close();
                }
            }
            return data;
        }


        public OrderDetail? GetDetail(int orderID, int productID)
        {
            OrderDetail? data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT od.*, p.ProductName, p.Photo, p.Unit
                    FROM OrderDetails AS od
                    JOIN Products AS p ON od.ProductID = p.ProductID
                    WHERE od.OrderID = @OrderID AND od.ProductID = @ProductID";

                var parameters = new
                {
                    OrderID = orderID,
                    ProductID = productID
                };

                data = connection.QueryFirstOrDefault<OrderDetail>(sql, parameters);
            }
            return data;
        }


        public IList<Order> List(int page = 1, int pageSize = 0, int status = 0, DateTime? fromTime = null, DateTime? toTime = null, string searchValue = "")
        {
            List<Order> list = new List<Order>();
            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";

            using (var connection = OpenConnection())
            {
                var sql = @"WITH cte AS
                    (
                        SELECT ROW_NUMBER() OVER (ORDER BY o.OrderTime DESC) AS RowNumber, o.*,
                                c.CustomerName, c.ContactName AS CustomerContactName,
                                c.Address AS CustomerAddress, c.Phone AS CustomerPhone,
                                c.Email AS CustomerEmail, e.FullName AS EmployeeName,
                                s.ShipperName, s.Phone AS ShipperPhone
                        FROM Orders AS o
                        LEFT JOIN Customers AS c ON o.CustomerID = c.CustomerID
                        LEFT JOIN Employees AS e ON o.EmployeeID = e.EmployeeID
                        LEFT JOIN Shippers AS s ON o.ShipperID = s.ShipperID
                        WHERE (@Status = 0 OR o.Status = @Status)
                            AND (@FromTime IS NULL OR o.OrderTime >= @FromTime)
                            AND (@ToTime IS NULL OR o.OrderTime <= @ToTime)
                            AND (@SearchValue = N'' OR c.CustomerName LIKE @SearchValue
                                OR e.FullName LIKE @SearchValue
                                OR s.ShipperName LIKE @SearchValue)
                    )
                    SELECT * FROM cte
                    WHERE (@PageSize = 0) OR (RowNumber BETWEEN (@Page - 1) * @PageSize + 1 AND @Page * @PageSize)";
                var parameters = new
                {
                    Page = page,
                    PageSize = pageSize,
                    Status = status,
                    searchValue = searchValue ?? "",
                    fromTime = fromTime,
                    toTime = toTime,
                };
                list = connection.Query<Order>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text).ToList();
                connection.Close();
            }
            return list;
        }

        public IList<OrderDetail> ListDetails(int orderID)
        {
            List<OrderDetail> list = new List<OrderDetail>();

            using (var connection = OpenConnection())
            {
                var sql = @"
            SELECT od.*, p.ProductName, p.Photo, p.Unit
            FROM OrderDetails AS od
            JOIN Products AS p ON od.ProductID = p.ProductID
            WHERE od.OrderID = @OrderID";

                list = connection.Query<OrderDetail>(sql, new { OrderID = orderID }).ToList();
            }

            return list;
        }


        public IList<StatusOrder> ListOfStatus()
        {
            List<StatusOrder> data = new List<StatusOrder>();

            using (var connection = OpenConnection())
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"SELECT * FROM OrderStatus";
                cmd.CommandType = CommandType.Text;

                using (var dbReader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (dbReader.Read())
                    {
                        data.Add(new StatusOrder()
                        {
                            Status = Convert.ToInt32(dbReader["Status"]),
                            Description = Convert.ToString(dbReader["Description"]),
                        });
                    }
                    dbReader.Close();
                }

                connection.Close();
            }
            return data;
        }

        public bool SaveDetail(int orderID, int productID, int quantity, decimal salePrice)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"IF EXISTS(SELECT * FROM OrderDetails WHERE OrderID = @OrderID AND ProductID = @ProductID)
                                UPDATE OrderDetails
                                SET Quantity = @Quantity, SalePrice = @SalePrice
                                WHERE OrderID = @OrderID AND ProductID = @ProductID
                            ELSE
                                INSERT INTO OrderDetails (OrderID, ProductID, Quantity, SalePrice)
                                VALUES (@OrderID, @ProductID, @Quantity, @SalePrice)";

                var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@OrderID", orderID);
                command.Parameters.AddWithValue("@ProductID", productID);
                command.Parameters.AddWithValue("@Quantity", quantity);
                command.Parameters.AddWithValue("@SalePrice", salePrice);

                result = command.ExecuteNonQuery() > 0;
            }
            return result;
        }

        public bool Update(Order data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"update Orders
                            set CustomerID = @CustomerID,
                                OrderTime = @OrderTime,
                                DeliveryProvince = @DeliveryProvince,
                                DeliveryAddress = @DeliveryAddress,
                                EmployeeID = @EmployeeID,
                                AcceptTime = @AcceptTime,
                                ShipperID = @ShipperID,
                                ShippedTime = @ShippedTime,
                                FinishedTime = @FinishedTime,
                                Status = @Status
                            where OrderID = @OrderID";
                var parameters = new
                {
                    CustomerID = data.CustomerID,
                    OrderTime = data.OrderTime,
                    DeliveryProvince = data.DeliveryProvince ?? "",
                    DeliveryAddress = data.DeliveryAddress ?? "",
                    EmployeeID = data.EmployeeID,
                    AcceptTime = data.AcceptTime,
                    ShipperID = data.ShipperID,
                    ShippedTime = data.ShippedTime,
                    FinishedTime = data.FinishedTime,
                    Status = data.Status,
                    OrderID = data.OrderID
                };
                result = connection.Execute(sql, parameters, commandType: CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }

        public bool UpdateAddress(string Province, string Address, int id)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                string sql = @"UPDATE Orders
                       SET DeliveryProvince = @Province,
                           DeliveryAddress = @Address
                       WHERE OrderID = @ID";
                var parameters = new
                {
                    Province = Province,
                    Address = Address,
                    ID = id
                };

                try
                {
                    result = connection.Execute(sql, parameters) > 0;
                }
                catch (Exception ex)
                {
                    // Xử lý các ngoại lệ (ví dụ: log lỗi)
                    Console.WriteLine("Lỗi khi cập nhật địa chỉ: " + ex.Message);
                }
            }
            return result;
        }

    }


}
