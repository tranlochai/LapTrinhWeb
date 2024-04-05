using SV20T1020025.DataLayers;
using SV20T1020025.DataLayers.SQLServer;
using SV20T1020025.DomainModels;
using System.Collections.Generic;
using System.Linq;

namespace SV20T1020025.BusinessLayers
{
    /// <summary>
    /// Lớp cung cấp dịch vụ dành cho mặt hàng
    /// </summary>
    public static class ProductDataService
    {
        private static readonly IProductDAL productDB;

        /// <summary>
        /// Constructor
        /// </summary>
        static ProductDataService()
        {
            productDB = new ProductDAL(Configuration.ConnectionString);
        }

        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng (không phân trang)
        /// </summary>
        /// <param name="searchValue">Giá trị tìm kiếm</param>
        /// <returns>Danh sách các mặt hàng</returns>
        public static List<Product> ListProducts(string searchValue = "")
        {
            return productDB.List(searchValue: searchValue).ToList();
        }

        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng dưới dạng phân trang
        /// </summary>
        /// <param name="rowCount">Số lượng bản ghi</param>
        /// <param name="page">Trang hiện tại</param>
        /// <param name="pageSize">Số lượng bản ghi trên mỗi trang</param>
        /// <param name="searchValue">Giá trị tìm kiếm</param>
        /// <param name="categoryId">ID danh mục</param>
        /// <param name="supplierId">ID nhà cung cấp</param>
        /// <param name="minPrice">Giá tối thiểu</param>
        /// <param name="maxPrice">Giá tối đa</param>
        /// <returns>Danh sách các mặt hàng</returns>
        public static List<Product> ListOfProducts(out int rowCount, int page = 1, int pageSize = 0,
                                                string searchValue = "", int categoryID = 0, int supplierID = 0,
                                                decimal minPrice = 0, decimal maxPrice = 0)
        {
            rowCount = productDB.Count(searchValue, categoryID, supplierID, minPrice, maxPrice);
            return productDB.List(page, pageSize, searchValue, categoryID, supplierID, minPrice, maxPrice).ToList();
        }



        /// <summary>
        /// Lấy thông tin một mặt hàng theo mã mặt hàng
        /// </summary>
        /// <param name="productId">ID mặt hàng</param>
        /// <returns>Thông tin mặt hàng</returns>
        public static Product? GetProduct(int productID)
        {
            return productDB.Get(productID);
        }

        /// <summary>
        /// Thêm một mặt hàng mới
        /// </summary>
        /// <param name="product">Thông tin mặt hàng cần thêm</param>
        /// <returns>ID của mặt hàng đã thêm</returns>
        public static int AddProduct(Product product)
        {
            return productDB.Add(product);
        }

        /// <summary>
        /// Cập nhật thông tin một mặt hàng
        /// </summary>
        /// <param name="product">Thông tin mặt hàng cần cập nhật</param>
        /// <returns>Kết quả cập nhật (true nếu thành công, ngược lại là false)</returns>
        public static bool UpdateProduct(Product data)
        {
            return productDB.Update(data);
        }

        /// <summary>
        /// Xóa một mặt hàng
        /// </summary>
        /// <param name="productId">ID của mặt hàng cần xóa</param>
        /// <returns>Kết quả xóa (true nếu thành công, ngược lại là false)</returns>
        public static bool DeleteProduct(int productID)
        {
            if (productDB.InUsed(productID))
                return false;
            return productDB.Delete(productID);
        }

        public static bool InUsedProduct ( int productID)
        {
            return productDB.InUsed(productID);
        }


        /// <summary>
        /// Lấy danh sách các ảnh của mặt hàng
        /// </summary>
        /// <param name="productId">ID của mặt hàng</param>
        /// <returns>Danh sách các ảnh của mặt hàng</returns>
        public static List<ProductPhoto> ListPhotos(int productID)
        {
            return productDB.ListPhotos(productID).ToList();
        }

        /// <summary>
        /// Lấy thông tin một ảnh của mặt hàng
        /// </summary>
        /// <param name="photoId">ID của ảnh</param>
        /// <returns>Thông tin ảnh</returns>
        public static ProductPhoto? GetPhoto(long photoID)
        {
            return productDB.GetPhoto(photoID);
        }

        /// <summary>
        /// Thêm một ảnh mới cho mặt hàng
        /// </summary>
        /// <param name="photo">Thông tin ảnh cần thêm</param>
        /// <returns>ID của ảnh đã thêm</returns>
        public static long AddPhoto(ProductPhoto data)
        {
            return productDB.AddPhoto(data);
        }

        /// <summary>
        /// Cập nhật thông tin một ảnh của mặt hàng
        /// </summary>
        /// <param name="photo">Thông tin ảnh cần cập nhật</param>
        /// <returns>Kết quả cập nhật (true nếu thành công, ngược lại là false)</returns>
        public static bool UpdatePhoto(ProductPhoto photo)
        {
            return productDB.UpdatePhoto(photo);
        }

        /// <summary>
        /// Xóa một ảnh của mặt hàng
        /// </summary>
        /// <param name="photoId">ID của ảnh cần xóa</param>
        /// <returns>Kết quả xóa (true nếu thành công, ngược lại là false)</returns>
        public static bool DeletePhoto(long photoID)
        {
            return productDB.DeletePhoto(photoID);
        }

        /// <summary>
        /// Lấy danh sách các thuộc tính của mặt hàng
        /// </summary>
        /// <param name="productId">ID của mặt hàng</param>
        /// <returns>Danh sách các thuộc tính của mặt hàng</returns>
        public static List<ProductAttribute> ListAttributes(int productID)
        {
            return productDB.ListAttributes(productID).ToList();
        }

        /// <summary>
        /// Lấy thông tin một thuộc tính của mặt hàng
        /// </summary>
        /// <param name="attributeId">ID của thuộc tính</param>
        /// <returns>Thông tin thuộc tính</returns>
        public static ProductAttribute? GetAttribute(int attributeID)
        {
            return productDB.GetAttribute(attributeID);
        }

        /// <summary>
        /// Thêm một thuộc tính mới cho mặt hàng
        /// </summary>
        /// <param name="attribute">Thông tin thuộc tính cần thêm</param>
        /// <returns>ID của thuộc tính đã thêm</returns>
        public static long AddAttribute(ProductAttribute attribute)
        {
            return productDB.AddAttribute(attribute);
        }

        /// <summary>
        /// Cập nhật thông tin một thuộc tính của mặt hàng
        /// </summary>
        /// <param name="attribute">Thông tin thuộc tính cần cập nhật</param>
        /// <returns>Kết quả cập nhật (true nếu thành công, ngược lại là false)</returns>
        public static bool UpdateAttribute(ProductAttribute attribute)
        {
            return productDB.UpdateAttribute(attribute);
        }

        /// <summary>
        /// Xóa một thuộc tính của mặt hàng
        /// </summary>
        /// <param name="attributeId">ID của thuộc tính cần xóa</param>
        /// <returns>Kết quả xóa (true nếu thành công, ngược lại là false)</returns>
        public static bool DeleteAttribute(long attributeID)
        {
            return productDB.DeleteAttribute(attributeID);
        }

    }
}
