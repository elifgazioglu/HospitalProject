using api.Data;
using HospitalProject.Models;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
namespace HospitalProject.UserContext

{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDBContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly string _nameIdentifier;


        public UserService(IHttpContextAccessor httpContextAccessor, ApplicationDBContext dBContext, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dBContext;
            _configuration = configuration;
            _nameIdentifier = _configuration["Jwt:NameIdentifier"];
        }

        string? IUserService.GetUserId()
        {
            var userIdentityMail = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdentityMail == null)
            {
                return "Email not found";
            }

            return Convert.ToString(userIdentityMail) ?? String.Empty;
        }
    }
}