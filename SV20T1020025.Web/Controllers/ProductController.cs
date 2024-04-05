using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV20T1020025.BusinessLayers;
using SV20T1020025.DomainModels;
using SV20T1020025.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace SV20T1020025.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Administrator}, {WebUserRoles.Employee}")]
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 20;
        private const string PRODUCT_SEARCH = "product_search"; // Tên biến dùng để lưu trong session

        public IActionResult Index()
        {
            // Lấy đầu vào tìm kiếm hiện đang lưu lại trong session
            PaginationSearchInput? input = ApplicationContext.GetSessionData<PaginationSearchInput>(PRODUCT_SEARCH);
            //Trường hợp tron session chưa có điều kiện thì tạo điều kiện mới
            if (input == null)
            {
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = ""
                };
            }

            return View(input);
        }

        public IActionResult Search(ProductSearchInput input)
        {
            int rowCount = 0;
            var data = ProductDataService.ListOfProducts(out rowCount, input.Page, input.PageSize,
                                                            input.SearchValue ?? "", input.CategoryID
                                                            , input.SupplierID, 0, 0);
            var model = new ProductSearchResult()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                RowCount = rowCount,
                CategoryID = input.CategoryID,
                SupplierID = input.SupplierID,
                Data = data
            };

            //Lưu lại đk tìm kiếm
            ApplicationContext.SetSessionData(PRODUCT_SEARCH, input);

            return View(model);
        }


        public IActionResult Create()
        {

            ViewBag.Title = "Bổ sung mặt hàng";
            ViewBag.IsEdit = false;
            var x = new Product()
            {
                ProductID = 0,
                Photo = "nophoto1.jpg",
                Price = 0,
                IsSelling = true
            };
            var model = new ProductDetails()
            {
                data = x
            };
            return View("Edit", model);
        }

        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật mặt hàng";
            ViewBag.IsEdit = true;
            var x = ProductDataService.GetProduct(id);

            if (x == null)
            {
                return BadRequest();
            }

            var model = new ProductDetails()
            {
                data = x,
                productPhotos = ProductDataService.ListPhotos(id),
                productAttributes = ProductDataService.ListAttributes(id)
            };
            if (model == null)
            {
                return RedirectToAction("Index");
            }
            if (string.IsNullOrWhiteSpace(model.data.Photo))
                model.data.Photo = "nophoto1.jpg";

            return View(model);
        }


        [HttpPost]
        public IActionResult Save(Product model, IFormFile? uploadPhoto = null)
        {


            //Xử lí ảnh upload: Nếu có ảnh được upload thì lưu anh lên server, gán tên file ảnh đã lưu cho model.Photo
            if (uploadPhoto != null)
            {
                //Tên file sẽ lưu trên server
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";
                //Đường dẫn đến file sẽ lưu trên server
                string filePath = Path.Combine(ApplicationContext.HostEnviroment.WebRootPath, @"images\products", fileName);

                //Lưu file lên server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadPhoto.CopyTo(stream);
                }

                //Gán tên file ảnh cho model.photo
                model.Photo = fileName;
            }

            if (string.IsNullOrWhiteSpace(model.ProductName))
                ModelState.AddModelError(nameof(model.ProductName), "Tên không được để trống");
            if (model.CategoryID == 0)
                ModelState.AddModelError(nameof(model.CategoryID), "Vui lòng chọn loại hàng");
            if (model.SupplierID == 0)
                ModelState.AddModelError(nameof(model.SupplierID), "Vui lòng chọn nhà cung cấp");
            if (string.IsNullOrWhiteSpace(model.Photo))
                ModelState.AddModelError(nameof(model.Photo), "Vui lòng chọn ảnh");
            if (string.IsNullOrWhiteSpace(model.Unit))
                ModelState.AddModelError(nameof(model.Unit), "Đơn vị tính không được để trống");
            if (string.IsNullOrWhiteSpace(model.Price.ToString()) || model.Price <= 0)
                ModelState.AddModelError(nameof(model.Price), "Giá bán không hợp lệ");


            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.ProductID == 0 ? "Bổ sung mặt hàng" : "Cập nhật thông tin mặt hàng";
                ViewBag.IsEdit = false;
                var model2 = new ProductDetails()
                {
                    data = model
                };
                return View("Edit", model2);
            }

            if (model.ProductID == 0)
            {
                int id = ProductDataService.AddProduct(model);
            }
            else
            {
                bool result = ProductDataService.UpdateProduct(model);
                if (!result)
                {
                    ModelState.AddModelError("Error", "Không cập nhật được thông tin mặt hàng.");
                    ViewBag.Title = "Cập nhật thông tin mặt hàng";
                    ViewBag.IsEdit = true;
                    return View("Edit", model);
                }
            }
            return RedirectToAction("Index");
        }





        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                ProductDataService.DeleteProduct(id);
                return RedirectToAction("Index");
            }
            var model = ProductDataService.GetProduct(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.AllowDelete = !ProductDataService.InUsedProduct(id);
            return View(model);
        }


        //[Route("photo/{method?}/{productID?}/{photoID?}")]
        public IActionResult Photo(int id = 0, string method = "", int photoID = 0)
        {
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung ảnh cho mặt hàng";
                    var model1 = new ProductPhoto()
                    {
                        ProductID = id,
                        PhotoID = photoID,
                        Photo = "nophoto.png"
                    };
                    return View(model1);
                case "edit":
                    ViewBag.Title = "Cập nhật ảnh cho mặt hàng";
                    var model2 = ProductDataService.GetPhoto(photoID);
                    if (model2 == null)
                    {
                        return RedirectToAction("Edit", id);
                    }
                    return View(model2);
                case "delete":
                    // Xóa  ảnh có mã PhotoID ( xóa trực tiếp, ko cần xác nhận)
                    bool result = ProductDataService.DeletePhoto(photoID);
                    return RedirectToAction("Edit", new { id = id });
                default:
                    return RedirectToAction("Index");
            }

        }





        [HttpPost]
        public IActionResult SavePhoto(ProductPhoto model, IFormFile? uploadPhoto = null)
        {


            //Xử lí ảnh upload: Nếu có ảnh được upload thì lưu anh lên server, gán tên file ảnh đã lưu cho model.Photo
            if (uploadPhoto != null)
            {
                //Tên file sẽ lưu trên server
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";
                //Đường dẫn đến file sẽ lưu trên server
                string filePath = Path.Combine(ApplicationContext.HostEnviroment.WebRootPath, @"images\products", fileName);

                //Lưu file lên server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadPhoto.CopyTo(stream);
                }

                //Gán tên file ảnh cho model.photo
                model.Photo = fileName;
            }


            if (model.DisplayOrder <= 0)
                ModelState.AddModelError(nameof(model.DisplayOrder), "Thứ tự hiển thị không hợp lệ");


            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.PhotoID == 0 ? "Bổ sung ảnh cho mặt hàng" : "Cập nhật ảnh cho mặt hàng";
                return View("Photo", model);
            }

            if (model.PhotoID == 0)
            {
                long id = ProductDataService.AddPhoto(model);
            }
            else
            {
                bool result = ProductDataService.UpdatePhoto(model);

            }
            return RedirectToAction("Edit", new { id = model.ProductID });
        }





        public IActionResult Attribute(int id = 0, string method = "", int attributeID = 0)
        {
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung thuộc tính cho mặt hàng";
                    var model1 = new ProductAttribute()
                    {
                        ProductID = id,
                        AttributeID = attributeID,
                    };
                    return View(model1);
                case "edit":
                    ViewBag.Title = "Cập nhật thuộc tính cho mặt hàng";
                    var model2 = ProductDataService.GetAttribute(attributeID);
                    if (model2 == null)
                    {
                        return RedirectToAction("Edit", new { id = id });
                    }
                    return View(model2);
                case "delete":
                    // Xóa thuộc tính có mã AttributeID ( xóa trực tiếp, ko cần xác nhận)
                    bool result = ProductDataService.DeleteAttribute(attributeID);
                    return RedirectToAction("Edit", new { id = id });
                default:
                    return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult SaveAttribute(ProductAttribute model)
        {
            if (string.IsNullOrWhiteSpace(model.AttributeName))
                ModelState.AddModelError(nameof(model.AttributeName), "Tên không được để trống");
            if (string.IsNullOrWhiteSpace(model.AttributeValue))
                ModelState.AddModelError(nameof(model.AttributeValue), "Giá trị thuộc tính không được để trống");
            if (model.DisplayOrder <= 0)
                ModelState.AddModelError(nameof(model.DisplayOrder), "Thứ tự không hợp lệ");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.AttributeID == 0 ? "Bổ sung thuộc tính cho mặt hàng" : "Cập nhật thuộc tính cho mặt hàng";
                return View("Attribute", model);
            }

            if (model.AttributeID == 0)
            {
                long id = ProductDataService.AddAttribute(model);
            }
            else
            {
                bool result = ProductDataService.UpdateAttribute(model);

            }
            return RedirectToAction("Edit", new { id = model.ProductID });
        }




    }
}
