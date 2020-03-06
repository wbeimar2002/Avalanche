using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Security.Server.Controllers.Resources;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avalanche.Security.Server.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public UsersController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserAsync([FromBody] UserCredentialsResource userCredentials)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<UserCredentialsResource, User>(userCredentials);
            
            var response = await _userService.CreateUserAsync(user, ERole.Common);
            if(!response.Success)
            {
                return BadRequest(response.Message);
            }

            var userResource = _mapper.Map<User, UserResource>(response.User);
            return Ok(userResource);
        }
    }
}