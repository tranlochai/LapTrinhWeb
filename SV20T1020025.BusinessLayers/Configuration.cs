using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV20T1020025.BusinessLayers
{
    /// <summary>
    /// khởi tạo, lưu trữ các thông tin cấu hình của BusinessLayer
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// chuỗi kết thông số kết nối đến csdl
        /// </summary>
        public static string ConnectionString { get; private set; } = "";
        /// <summary>
        /// Khởi tạo cấu hình cho BussinessLayer
        /// hàm này phải được gọi trước khi ứng dụng chạy
        /// </summary>
        /// <param name="connectString"></param>
        public static void Initialize(string connectString)
        {
            Configuration.ConnectionString = connectString;
        }
    }
}

//static class là gì? khác với class thông thường chỗ nào