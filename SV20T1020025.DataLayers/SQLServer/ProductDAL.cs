using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Dapper;
using SV20T1020025.DomainModels;
using System.Data;

namespace SV20T1020025.DataLayers.SQLServer
{
    public class ProductDAL : _BaseDAL, IProductDAL
    {
        public ProductDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Product data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"INSERT INTO Products (ProductName, ProductDescription, SupplierID, CategoryID,  Unit, Price, Photo, IsSelling) 
                    VALUES (@ProductName, @ProductDescription, @SupplierID, @CategoryID,  @Unit, @Price, @Photo, @IsSelling);
                    SELECT @@IDENTITY;";
                var parameters = new
                {
                    ProductName = data.ProductName ?? "",
                    ProductDescription = data.ProductDescription ?? "",
                    SupplierID = data.SupplierID,
                    CategoryID = data.CategoryID,
                    Unit = data.Unit,
                    Price = data.Price,
                    Photo = data.Photo,
                    IsSelling = data.IsSelling
                };
                id = connection.ExecuteScalar<int>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
                connection.Close();
            }
            return id;
        }


        public int Count(string searchValue = "", int categoryID = 0, int supplierID = 0, 
                            decimal minPrice = 0, decimal maxPrice = 0)
        {
            int count = 0;
            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT COUNT(*) FROM Products
                    WHERE (@searchValue = N'' OR ProductName LIKE @searchValue)
                        AND (@SupplierID = 0 OR SupplierID = @SupplierID)
                        AND (@CategoryID = 0 OR CategoryID = @CategoryID)
                            and (Price >= @MinPrice)
                            and (@MaxPrice <= 0 or Price <= @MaxPrice)";
                var parameters = new
                {
                    searchValue,
                    SupplierID = supplierID,
                    CategoryID = categoryID,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };
                count = connection.ExecuteScalar<int>(sql, parameters);
            }
            return count;
        }


        public bool Delete(int id)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM Products WHERE ProductID = @ProductID";
                var parameters = new
                {
                    ProductId = id,
                };
                result = connection.Execute(sql: sql, param: parameters, commandType: System.Data.CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }

        public Product? Get(int id)
        {
            Product? data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM Products WHERE ProductID = @ProductID";
                var parameters = new { ProductId = id };
                data = connection.QueryFirstOrDefault<Product>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
                connection.Close();
            }
            return data;
        }

        public bool InUsed(int id)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"if exists(select * from OrderDetails where ProductId = @productId)
                                select 1
                            else 
                                select 0";
                var parameters = new { ProductId = id };
                result = connection.ExecuteScalar<bool>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
                connection.Close();
            }
            return result;
        }

        public IList<Product> List(int page = 1, int pageSize = 0, string searchValue = "", 
                                    int supplierID = 0, int categoryID = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            List<Product> data = new List<Product>();
            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";

            using (var connection = OpenConnection())
            {
                var sql = @"with cte as
                    (
                        select  *,
                                row_number() over(order by ProductName) as RowNumber
                        from    Products
                        where   (@SearchValue = N'' or ProductName like @SearchValue)
                            and (@CategoryID = 0 or CategoryID = @CategoryID)
                            and (@SupplierID = 0 or SupplierId = @SupplierID)
                            and (Price >= @MinPrice)
                            and (@MaxPrice <= 0 or Price <= @MaxPrice)
                    )
                    select * from cte
                    where   (@PageSize = 0)
                        or (RowNumber between (@Page - 1) * @PageSize + 1 and @Page * @PageSize)";
                var parameters = new
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchValue = searchValue ?? "",
                    CategoryID = categoryID,
                    SupplierID = supplierID,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice
                };
                data = connection.Query<Product>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text).ToList();
            }
            return data;
        }



        public bool Update(Product data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"
            IF NOT EXISTS (SELECT * FROM Products WHERE ProductID <> @ProductID AND ProductName = @ProductName)
            BEGIN
                UPDATE Products 
                SET ProductName = @ProductName,
                    ProductDescription = @ProductDescription,
                    SupplierID = @SupplierID,
                    CategoryID = @CategoryID,
                    Unit = @Unit, 
                    Price = @Price,
                    Photo = @Photo,
                    IsSelling = @IsSelling
                WHERE ProductID = @ProductID;
            END";
                var parameters = new
                {
                    ProductID = data.ProductID,
                    ProductName = data.ProductName ?? "",
                    ProductDescription = data.ProductDescription ?? "",
                    SupplierID = data.SupplierID,
                    CategoryID = data.CategoryID,
                    Unit = data.Unit ?? "",
                    Price = data.Price,
                    Photo = data.Photo ?? "",
                    IsSelling = data.IsSelling
                };
                result = connection.Execute(sql, parameters, commandType: CommandType.Text) > 0;
            }
            return result;
        }






        // Các phương thức cho Attribute
        public IList<ProductAttribute> ListAttributes(int productID)
        {
            List<ProductAttribute> attributes = new List<ProductAttribute>();
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM ProductAttributes WHERE ProductID = @ProductID";
                attributes = connection.Query<ProductAttribute>(sql, new { ProductID = productID }).ToList();
            }
            return attributes;
        }

        public ProductAttribute? GetAttribute(long id)
        {
            ProductAttribute? data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM ProductAttributes WHERE AttributeID = @attributeID";
                var parameters = new { AttributeId = id };
                data = connection.QueryFirstOrDefault<ProductAttribute>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
                connection.Close();
            }
            return data;
        }


        public long AddAttribute(ProductAttribute data)
        {
            long id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"INSERT INTO ProductAttributes (ProductID, AttributeName, AttributeValue, DisplayOrder) 
                    VALUES (@ProductID, @AttributeName, @AttributeValue, @DisplayOrder);
                    SELECT @@IDENTITY;";
                var parameters = new
                {
                    ProductID = data.ProductID,
                    AttributeName = data.AttributeName ?? "",
                    AttributeValue = data.AttributeValue ?? "",
                    DisplayOrder = data.DisplayOrder
                };
                id = connection.ExecuteScalar<long>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
            }
            return id;
        }


        public bool UpdateAttribute(ProductAttribute data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"
                                    update ProductAttributes 
                                    set AttributeName = @AttributeName,
                                        AttributeValue = @AttributeValue,
                                        ProductID = @ProductID,
                                        DisplayOrder = @DisplayOrder
                                   where AttributeID = @AttributeID
                               ";
                var parameters = new
                {
                    AttributeName = data.AttributeName ?? "",
                    AttributeValue = data.AttributeValue ?? "",
                    ProductID = data.ProductID,
                    DisplayOrder = data.DisplayOrder,
                    AttributeID = data.AttributeID

                };
                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
            }

            return result;
        }



        public bool DeleteAttribute(long id)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM ProductAttributes WHERE AttributeID = @AttributeID";
                var parameters = new
                {
                    AttributeID = id,
                };
                result = connection.Execute(sql: sql, param: parameters, commandType: System.Data.CommandType.Text) > 0;
            }
            return result;
        }


        // Các phương thức cho Photo
        public IList<ProductPhoto> ListPhotos(int productID)
        {
            List<ProductPhoto> data = new List<ProductPhoto>();
            using (SqlConnection cn = OpenConnection())
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = @"SELECT * FROM ProductPhotos WHERE ProductID = @ProductID";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = cn;

                cmd.Parameters.AddWithValue("@ProductID", productID);

                var dbReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (dbReader.Read())
                {
                    data.Add(new ProductPhoto()
                    {
                        PhotoID = Convert.ToInt32(dbReader["PhotoID"]),
                        ProductID = Convert.ToInt32(dbReader["ProductID"]),
                        Photo = Convert.ToString(dbReader["Photo"]),
                        Description = Convert.ToString(dbReader["Description"]),
                        IsHidden = Convert.ToBoolean(dbReader["IsHidden"]),
                        DisplayOrder = Convert.ToInt32(dbReader["DisplayOrder"]),
                    });
                }
                dbReader.Close();
                cn.Close();
            }

            return data;
        }

        public ProductPhoto? GetPhoto(long photoID)
        {
            ProductPhoto photo = null;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM ProductPhotos WHERE PhotoID = @PhotoID";
                photo = connection.QueryFirstOrDefault<ProductPhoto>(sql, new { PhotoID = photoID });
            }
            return photo;
        }

        public long AddPhoto(ProductPhoto data)
        {
            long id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"INSERT INTO ProductPhotos (ProductID, Photo, Description, DisplayOrder, IsHidden) 
                    VALUES (@ProductID, @Photo, @Description, @DisplayOrder, @IsHidden);
                    SELECT @@IDENTITY;";
                var parameters = new
                {
                    ProductID = data.ProductID,
                    Photo = data.Photo ?? "",
                    Description = data.Description ?? "",
                    DisplayOrder = data.DisplayOrder,
                    IsHidden = data.IsHidden
                };
                id = connection.ExecuteScalar<long>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
            }
            return id;
        }


        public bool UpdatePhoto(ProductPhoto data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"
                                    update ProductPhotos 
                                    set Photo = @Photo,
                                        Description = @Description,
                                        ProductID = @ProductID,
                                        DisplayOrder = @DisplayOrder,
                                        IsHidden = @IsHidden
                                   where PhotoID = @PhotoID
                               ";
                var parameters = new
                {
                    Photo = data.Photo ?? "",
                    Description = data.Description ?? "",
                    ProductID = data.ProductID,
                    DisplayOrder = data.DisplayOrder,
                    IsHidden = data.IsHidden,
                    PhotoID = data.PhotoID

                };
                result = connection.Execute(sql: sql, param: parameters, commandType: CommandType.Text) > 0;
            }

            return result;
        }




        public bool DeletePhoto(long photoID)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM ProductPhotos WHERE PhotoID = @PhotoID";
                var parameters = new
                {
                    PhotoID = photoID,
                };
                result = connection.Execute(sql: sql, param: parameters, commandType: System.Data.CommandType.Text) > 0;
            }
            return result;
        }
    }
}
