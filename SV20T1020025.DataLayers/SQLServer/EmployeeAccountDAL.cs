using Dapper;
using Microsoft.VisualBasic;
using SV20T1020025.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV20T1020025.DataLayers.SQLServer
{
    public class EmployeeAccountDAL : _BaseDAL, IUserAccountDAL
    {
        public EmployeeAccountDAL(string connectionString) : base(connectionString)
        {
        }
        public UserAccount? Authorize(string userName, string password)
        {
            UserAccount? data = null;
            using (var cn = OpenConnection())
            {
                var sql = @"select EmployeeID as UserID, Email as UserName, FullName, Email, Photo, Password, RoleNames
                            from Employees where Email = @Email AND Password = @Password";
                var parameters = new
                {
                    Email = userName,
                    Password = password,
                };
                data = cn.QuerySingleOrDefault<UserAccount>(sql, parameters);
                cn.Close();
            }
            return data;
        }
        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            bool result = false;
            using (var cn = OpenConnection())
            {
                var sql = @"update Employees
                            set Password = @NewPassword
                            where Email = @Email and Password = @OldPassword";
                var parameters = new
                {
                    Email = userName,
                    OldPassword = oldPassword,
                    NewPassword = newPassword
                };
                result = cn.Execute(sql, parameters) > 0;
                cn.Close();
            }
            return result;
        }

        public bool VerifyPassword(string userId, string password)
        {
            bool result = false;
            using (var cn = OpenConnection())
            {
                var sql = @"select 1 from Employees where EmployeeID = @UserID and Password = @Password";
                var parameters = new
                {
                    UserID = userId,
                    Password = password
                };
                result = cn.ExecuteScalar<int>(sql, parameters) == 1;
                cn.Close();
            }
            return result;
        }

    }
}
