using SV20T1020025.DataLayers.SQLServer;
using SV20T1020025.DataLayers;
using SV20T1020025.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV20T1020025.BusinessLayers
{
    public static class UserAccountService
    {


        private static readonly IUserAccountDAL employeeAccountDB;
        private static readonly IUserAccountDAL customerAccountDB;

        static UserAccountService()
        {
            //string connectionString = Configuration.ConnectionString;
            //employeeAccountDB = new EmployeeAccountDAL(connectionString);

            employeeAccountDB = new EmployeeAccountDAL(Configuration.ConnectionString);
            customerAccountDB = new CustomerAccountDAL(Configuration.ConnectionString);
        }
        public static UserAccount? Authorize(string userName, string password)
        {
            // Gọi phương thức Authorize từ IUserAccountDAL
            var employeeResult = employeeAccountDB.Authorize(userName, password);
            if (employeeResult != null)
            {
                return employeeResult;
            }

            return customerAccountDB.Authorize(userName, password);
        }


        public static bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            // Gọi phương thức ChangePassword từ IUserAccountDAL cho cả nhân viên và khách hàng
            return employeeAccountDB.ChangePassword(userName, oldPassword, newPassword) ||
                   customerAccountDB.ChangePassword(userName, oldPassword, newPassword);
        }

        public static bool VerifyPassword(string userId, string password)
        {
            // Gọi phương thức VerifyPassword từ IUserAccountDAL cho cả nhân viên và khách hàng
            return employeeAccountDB.VerifyPassword(userId, password) || customerAccountDB.VerifyPassword(userId, password);
        }


    }
}
