using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoAPI.DataAccess;
using ProjetoAPI.DTOs;
using ProjetoAPI.Models;
using System.Security.Cryptography;
using System.Text;

namespace ProjetoAPI.Controllers
{
    [Route("api/conta")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            this._context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody]RegisterDTO registerDTO)
        {
            if ((await _context.Users.SingleOrDefaultAsync(usuario => usuario.UserName == registerDTO.UserName) != null))
            {
                return BadRequest("The username already exists!");
            }

            using var hmac = new HMACSHA512();

            var user = new User
            {
                UserName = registerDTO.UserName,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt = hmac.Key,
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody]LoginDTO loginDTO)
        {
            var user = await _context.Users.SingleOrDefaultAsync(user => user.UserName == loginDTO.UserName);

            if (user == null) { return Unauthorized("Invalid username!"); }

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

            for (int i=0;  i<hash.Length; i++) 
            {
                if (hash[i] != user.PasswordHash[i]) { return Unauthorized("Invalid password!"); }
            }

            return user;
        }
    }
}
