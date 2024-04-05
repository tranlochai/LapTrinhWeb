using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020025.BusinessLayers;
using SV20T1020025.Web.Models;
using SV20T1020025.DomainModels;
using System.Drawing.Printing;

namespace SV20T1020025.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Employee}")]
    public class OrderController : Controller
    {
        // số dòng trên 1 trang khi hiển thị danh sách đơn hàng
        private const int ORDER_PAGE_SIZE = 20;
        //tên biến session để lưu điều kiện tìm kiếm đơn hàng
        private const string ORDER_SEARCH = "order_search";

        
        //số dòng trên 1 trang khi hiển thị  danh sách mặt hàng cần tìm khi lập đơn hàng
        private const int PRODUCT_PAGE_SIZE = 5;
        //tên biến session lưu điều kiện tìm kiếm mặt hàng khi lập đơn hàng
        private const string PRODUCT_SEARCH = "product_search_for_sale";
        //tên biến session dùng để lưu giỏ hàng
        private const string SHOPPING_CART = "shopping_cart";


        public IActionResult Index()
        {
            // Lấy đầu vào tìm kiếm hiện đang lưu lại trong session
            OrderSearchInput? input = ApplicationContext.GetSessionData<OrderSearchInput>(ORDER_SEARCH);
            //Trường hợp tron session chưa có điều kiện thì tạo điều kiện mới
            if (input == null)
            {
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = ORDER_PAGE_SIZE,
                    SearchValue = "",
                    Status = 0,
                    DateRange = string.Format("{0:dd/MM/yyyy}",
                                            DateTime.Today.AddMonths(-1),
                                            DateTime.Today)
                };
            }

            return View(input);
        }


        /// <summary>
        /// Thực hiện chức năng tìm kiếm đơn hàng
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IActionResult Search(OrderSearchInput input)
        {
            int rowCount = 0;
            List<Order> data = OrderDataService.ListOrders(out rowCount, input.Page, input.PageSize,
                                                    input.Status, input.FromTime, input.ToTime, input.SearchValue ?? "");
            var model = new OrderSearchResult()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                Status = input.Status,
                TimeRange = input.DateRange ?? "", // Sử dụng DateRange để gán giá trị cho TimeRange
                RowCount = rowCount,
                Data = data
            };
            //Lưu lại điều kiện tìm kiếm vào trong session
            ApplicationContext.SetSessionData(ORDER_SEARCH, input);

            return View(model);
        }



        public IActionResult Details(int id = 0)
        {
            var order = OrderDataService.GetOrder(id);
            if (order == null)
                return RedirectToAction("Index");
            var details = OrderDataService.ListOrderDetails(id);
            var model = new OrderDetailModel()
            {
                Order = order,
                Details = details
            };
            return View(model);
        }


        /// <summary>
        /// chuyển đơn hàng sang trạng thái đã được duyệt
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <returns></returns>
        public IActionResult Accept(int id = 0)
        {
            bool result = OrderDataService.AcceptOrder(id);
            if (!result)
                TempData["Message"] = "Không thể duyệt đơn hàng này";

            return RedirectToAction("Details", new { id });
        }

        /// <summary>
        /// Chuyển đơn hàng sang trạng thái kết thúc
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Finish(int id = 0)
        {
            bool result = OrderDataService.FinishOrder(id);
            if (!result)
                TempData["Message"] = "Không thể ghi nhận trạng thái kết thúc cho đơn hàng này";

            return RedirectToAction("Details", new { id });
        }



        /// <summary>
        /// chuyển đơn hàng sang trạng thái bị huỷ
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <returns></returns>
        public IActionResult Cancel(int id = 0)
        {
            bool result = OrderDataService.CancelOrder(id);
            if (!result)
                TempData["Message"] = "Không thể thực hiện thao tác huỷ đối với đơn hàng này";
            return RedirectToAction("Details", new { id });
        }


        /// <summary>
        /// chuyển đơn hàng sang trạng thái bị từ chối 
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <returns></returns>
        public IActionResult Reject(int id = 0)
        {
            bool result = OrderDataService.RejectOrder(id);
            if (!result)
                TempData["Message"] = "Không thể thực hiện thao tác từ chối đối với đơn hàng này";

            return RedirectToAction("Details", new { id });
        }

        /// <summary>
        /// xoá đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <returns></returns>
        public IActionResult Delete(int id = 0)
        {
            bool result = OrderDataService.DeleteOrder(id);
            if (!result)
            {
                TempData["Message"] = "Không thể xoá đơn hàng này";
                return RedirectToAction("Details", new { id });
            }
            return RedirectToAction("Index");
        }


        /// <summary>
        /// giao diện để chọn người giao hàng cho đơn hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Shipping(int id = 0)
        {
            ViewBag.OrderID = id;
            return View();
        }


        /// <summary>
        /// ghi nhận người giao hàng cho đơn hàng và chuyển đơn hàng sang trạng thái đang giao hàng
        /// hàm trả về chuỗi khác rỗng thông báo lỗi nếu đầu vào không hợp lệ hoặc lỗi 
        /// hàm trả về chuỗi rỗng nếu thành công
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <param name="shippperID">Mã người giao hàng</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Shipping(int id = 0, int shipperID = 0)
        {
            if (shipperID <= 0)
                return Json("Vui lòng chọn người giao hàng");
            bool result = OrderDataService.ShipOrder(id, shipperID);
            if (!result)
                return Json("Đơn hàng không cho phép chuyển cho người giao hàng");
            return RedirectToAction("Details", new { id = id });
        }



        /// <summary>
        /// giao diện trang lập đơn hàng mới 
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH);
            if (input == null)
            {
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = PRODUCT_PAGE_SIZE,
                    SearchValue = ""
                };

            }
            return View(input);
        }

        /// <summary>
        /// Tìm kiếm mặt hàng để đưa vào giỏ hàng
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IActionResult SearchProduct(ProductSearchInput input)
        {
            int rowCount = 0;
            var data = ProductDataService.ListOfProducts(out rowCount, input.Page, input.PageSize,
                                                         input.SearchValue ?? "");
            var model = new ProductSearchResult()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                RowCount = rowCount,
                Data = data
            };
            ApplicationContext.SetSessionData(PRODUCT_SEARCH, input);
            return View(model);

        }

        /// <summary>
        /// Lấy giỏ hàng hiện đang lưu trong session
        /// </summary>
        /// <returns></returns>
        public List<OrderDetail> GetShoppingCart()
        {
            var shoppingCart = ApplicationContext.GetSessionData<List<OrderDetail>>(SHOPPING_CART);
            if (shoppingCart == null)
            {
                shoppingCart = new List<OrderDetail>();
                ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            }
            return shoppingCart; 
        }

        /// <summary>
        /// Trang hiển thị danh sách các mặt hàng đang có trong giỏ hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult ShowShoppingCart()
        {
            var model = GetShoppingCart();
            return View(model);
        }


        /// <summary>
        /// bổ sung thêm mặt hàng vào giỏ hàng
        /// hàm trả về chuỗi khác rỗng thông báo lỗi nếu dữ liệu không hợp lệ
        /// hàm trả về chuỗi rỗng nếu thành công
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IActionResult AddToCart(OrderDetail data)
        {
            if (data.SalePrice <= 0 || data.Quantity <= 0)
                return Json("Giá bán và số lượng không hợp lệ");
            var shoppingCart = GetShoppingCart();
            var existsProduct = shoppingCart.FirstOrDefault(m => m.ProductID == data.ProductID);
            if (existsProduct == null) //nếu mặt hàng chưa có trong giỏ thì bổ sung thêm vào giỏ hàng
            {
                shoppingCart.Add(data);
            }
            else // Nếu mặt hàng đã có trong giỏ thì tăng số lượng và thay đổi giá bán
            {
                existsProduct.Quantity += data.Quantity;
                existsProduct.SalePrice += data.SalePrice;
            }
                ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
                return Json("");
        }



        /// <summary>
        /// xoá mặt hàng ra khỏi giỏ hàng
        /// </summary>
        /// <param name="id">Mã mặt hàng cần xoá khỏi giỏ hàng</param>
        /// <returns></returns>
        public IActionResult RemoveFromCart(int id = 0)
        {
            var shoppingCart = GetShoppingCart();
            int index = shoppingCart.FindIndex (m => m.ProductID == id);
            if (index >= 0) //nếu mặt hàng chưa có trong giỏ thì bổ sung thêm vào giỏ hàng
            {
                shoppingCart.RemoveAt(index);
            }
            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            return Json("");
        }

        /// <summary>
        /// xoá tất cả các mặt hàng trong giỏ hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult ClearCart()
        {
            var shoppingCart = GetShoppingCart();
            shoppingCart.Clear();
            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            return Json("");
        }


        /// <summary>
        /// khởi tạo đơn hàng (lập một đơn hàng mới)
        /// hàm trả về chuỗi khác rỗng thông báo lỗi nếu đầu vào không hợp lệ
        /// hoặc việc tạo đơn hàng không thành công
        /// ngược lại, hàm trả về mã của đơn hàng được tạo ( là một giá trị số)
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="deliveryProvince"></param>
        /// <param name="deliveryAddress"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Init(int customerID = 0, string deliveryProvince = "",
                                          string deliveryAddress = "")
        {
            var shoppingCart = GetShoppingCart();
            if (shoppingCart.Count == 0)
            {
                return Json("Giỏ hàng trống, không thể lập đơn hàng");
            }

            if (customerID <= 0 || string.IsNullOrWhiteSpace(deliveryAddress)
                        || string.IsNullOrWhiteSpace(deliveryProvince))
                return Json("Vui lòng nhập đầy đủ thông tin");

            int employeeID = Convert.ToInt32(User.GetUserData()?.UserId);
            int orderID = OrderDataService.InitOrder(employeeID, customerID, deliveryProvince,
                                                    deliveryAddress, shoppingCart);
            ClearCart();
            return Json(orderID);
        }



        /// <summary>
        /// xoá mặt hàng ra khỏi đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <param name="productId">Mã mặt hàng cần xoá</param>
        /// <returns></returns>
        public IActionResult DeleteDetail(int id = 0, int productId = 0)
        {
            bool result = OrderDataService.DeleteOrderDetail(id, productId);
            if (!result)
                TempData["Message"] = "Không thể xoá mặt hàng ra khỏi đơn hàng";
            return RedirectToAction("Details", new {id});
        }


        /// <summary>
        /// giao diện để sửa đổi thông tin mặt hàng được bán trong đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <param name="productId">Mã mặt hàng</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult EditDetail(int id = 0, int productId = 0)
        {
            var model = OrderDataService.GetOrderDetail(id, productId);
            return View(model);
        }


        [HttpPost]
        public IActionResult UpdateDetail(int orderID, int productID, int quantity, decimal salePrice)
        {
            if (quantity <= 0)
                return Json("Số lượng không hợp lệ");
            if (salePrice < 0)
                return Json("Giá bán không hợp lệ");

            bool result = OrderDataService.SaveOrderDetail(orderID, productID, quantity, salePrice);
            if (!result)
                return Json("Không được phép thay đổi thông tin của đơn hàng này");
            return RedirectToAction("Details", new { id = orderID });
        }

        [HttpPost]
        public IActionResult UpdateAddress(int id = 0, string Province = "", string Address = "")
        {
            if (string.IsNullOrWhiteSpace(Address))
                ModelState.AddModelError("Address", "Địa chỉ không được để trống");

            if (!ModelState.IsValid)
            {
                var order = OrderDataService.GetOrder(id);
                if (order == null)
                    return RedirectToAction("Index");
                var details = OrderDataService.ListOrderDetails(id);
                var model = new OrderDetailModel()
                {
                    Order = order,
                    Details = details
                };
                return View("Details", model);
            }

            bool result = false;

            if (id > 0)
            {
                Order item = OrderDataService.GetOrder(id);
                result = OrderDataService.UpdateAddress(Province, Address, id);


            }

            return RedirectToAction("Details", new { id = id });
        }



    }
}
