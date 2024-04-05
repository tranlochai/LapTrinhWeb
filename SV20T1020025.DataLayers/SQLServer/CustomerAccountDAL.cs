using Dapper;
using SV20T1020025.DomainModels;
using System;

namespace SV20T1020025.DataLayers.SQLServer
{
    public class CustomerAccountDAL : _BaseDAL, IUserAccountDAL
    {
        public CustomerAccountDAL(string connectionString) : base(connectionString)
        {
        }

        public UserAccount? Authorize(string userName, string password)
        {
            UserAccount? data = null;
            using (var cn = OpenConnection())
            {
                var sql = @"SELECT CustomerID AS UserID, Email AS UserName, CustomerName, Email, Password, RoleNames
                            FROM Customers WHERE Email = @Email AND Password = @Password";
                var parameters = new
                {
                    Email = userName,
                    Password = password,
                };
                data = cn.QuerySingleOrDefault<UserAccount>(sql, parameters);
            }
            return data;
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            bool result = false;
            using (var cn = OpenConnection())
            {
                var sql = @"UPDATE Customers
                            SET Password = @NewPassword
                            WHERE Email = @Email AND Password = @OldPassword";
                var parameters = new
                {
                    Email = userName,
                    OldPassword = oldPassword,
                    NewPassword = newPassword
                };
                result = cn.Execute(sql, parameters) > 0;
            }
            return result;
        }

        public bool VerifyPassword(string userId, string password)
        {
            bool result = false;
            using (var cn = OpenConnection())
            {
                var sql = @"select 1 from Customers where CustomerID = @UserID and Password = @Password";
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
