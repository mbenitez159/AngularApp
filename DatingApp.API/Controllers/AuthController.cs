using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository repo;
        private readonly IConfiguration configuration;
        public AuthController(IAuthRepository repo, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto user)
        {
            // validate request
            user.UserName = user.UserName.ToLower();
            if (await repo.UserExits(user.UserName))
                return BadRequest("User already exits");

            var userCreate = new User()
            {
                UserName = user.UserName,
            };
            var createUser = await repo.Register(userCreate, user.Password);
            //if(createUser == null)
            //return BadRequest("Something went wrong, when we trying to create user");
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto login)
        {
            var userLogin = await repo.login(login.UserName.ToLower(), login.Password);
            if (userLogin == null)
                return Unauthorized();
            var claims = new[]{
                new Claim(ClaimTypes.NameIdentifier,userLogin.Id.ToString()),
                new Claim(ClaimTypes.Name,userLogin.UserName),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(configuration.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor(){
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {
                token= tokenHandler.WriteToken(token)
            });

        }
    }
}