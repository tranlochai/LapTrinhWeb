using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV20T1020025.DomainModels
{
    /// <summary>
    /// người giao hàng
    /// </summary>
    public class Shipper
    {
        public int ShipperID { get; set; }
        public string ShipperName { get; set; } = "";
        public string Phone { get; set; } = "";

    }
}
