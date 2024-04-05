using SV20T1020025.DomainModels;

namespace SV20T1020025.Web.Models
{
    /// <summary>
    /// lớp cha cho các lớp biểu diễn dữ liệu kết quả tìm kiếm, phân trang
    /// </summary>
    public abstract class BasePaginationResult
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SearchValue { get; set; } = "";
        public int RowCount { get; set; }
        public int PageCount
        {
            get
            {
                if (PageSize == 0)
                    return 1;
                int c = RowCount / PageSize;
                if (RowCount % PageSize > 1)
                    c += 1;
                return c;
            }

        }

    }

    /// <summary>
    /// Biểu diễn kết quả tìm kiếm và lấy danh sách khách hàng
    /// </summary>
    public class CustomerSearchResult : BasePaginationResult
    {
        public List<Customer> Data { get; set; } = new List<Customer>();
    }

    /// <summary>
    /// Biểu diễn kết quả tìm kiếm và lấy danh sách loại hàng
    /// </summary>
    public class CategorySearchResult : BasePaginationResult
    {
        public List<Category> Data { get; set; } = new List<Category>();
    }

    /// <summary>
    /// Biểu diễn kết quả tìm kiếm và lấy danh sách nhà cung cấp
    /// </summary>
    public class SupplierSearchResult : BasePaginationResult
    {
        public List<Supplier> Data { get; set; } = new List<Supplier>();
    }

    /// <summary>
    /// Biểu diễn kết quả tìm kiếm và lấy danh sách người giao hàng
    /// </summary>
    public class ShipperSearchResult : BasePaginationResult
    {
        public List<Shipper> Data { get; set; } = new List<Shipper>();
    }

    /// <summary>
    /// Biểu diễn kết quả tìm kiếm và lấy danh sách nhân viên
    /// </summary>
    public class EmployeeSearchResult : BasePaginationResult
    {
        public List<Employee> Data { get; set; } = new List<Employee>();
    }

    public class ProductSearchResult : BasePaginationResult
    {
        public int CategoryID = 0;
        public int SupplierID = 0;

        public List<Product> Data { get; set; } = new List<Product>();
    }

    public class ProductDetails
    {
        public Product data { get; set; } = new Product();
        public List<ProductAttribute> productAttributes { get; set; } = new List<ProductAttribute>();
        public List<ProductPhoto> productPhotos { get; set; } = new List<ProductPhoto>();

    }

    public class CategoryDetails
    {
        public Category data { get; set; } = new Category();

    }
}

