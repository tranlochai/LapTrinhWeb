using Microsoft.AspNetCore.Mvc.Rendering;
using SV20T1020025.BusinessLayers;

namespace SV20T1020025.Web
{
    public class SelectListHelper
    {
        public static List<SelectListItem> Provinces()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "-- Chọn tỉnh/thành --"
            });
            foreach (var  item in CommonDataService.ListOfProvinces())
            {
                list.Add(new SelectListItem()
                {
                    Value = item.ProvinceName,
                    Text = item.ProvinceName
                });
            }
            return list;
        }

        public static List<SelectListItem> Status()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "---Trạng thái---",
            });

            foreach (var item in OrderDataService.ListOfStatus())
            {
                if (item.Description.Equals("Rejected"))
                    item.Description = "Đơn hàng bị từ chối";
                if (item.Description.Equals("Cancel"))
                    item.Description = "Đơn hàng bị hủy";
                if (item.Description.Equals("Init"))
                    item.Description = "Đơn hàng mới (chờ duyệt)";
                if (item.Description.Equals("Accepted"))
                    item.Description = "Đơn hàng đã duyệt (chờ chuyển hàng)";
                if (item.Description.Equals("Shipping"))
                    item.Description = "Đơn hàng đang được giao";
                if (item.Description.Equals("Finished"))
                    item.Description = "Đơn hàng đã hoàn tất thành công";

                list.Add(new SelectListItem()
                {
                    Value = Convert.ToString(item.Status),
                    Text = item.Description,
                });
            }

            return list;
        }
    }
}
