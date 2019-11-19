using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext context;
        public AuthRepository(DataContext context)
        {
            this.context = context;

        }
        public async Task<User> login(string username, string password)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserName.Equals(username));
            if(user == null)
                return null;
            if (!VerifyPasswordHash(password,user.PasswordHash,user.PasswordSalt))
                return null;
            return user;
        }


        public async Task<User> Register(User user, string password)
        {
            byte [] passwordHash, passwordSalt;
            CreatePasswordHash(password,out passwordHash,out passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            return user;
        }
        public async Task<bool> UserExits(string username)
        {
            if(await context.Users.AnyAsync(u => u.UserName.Equals(username)))
                return true;
            return false;
        }

        #region Password Hash
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac= new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac= new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i=0; i < computedHash.Length; i++)
                {
                    if(computedHash[i]!=passwordHash[i]) return false;
                }
            }
            return true;
        }
        #endregion
    }
}