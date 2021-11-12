using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Security.Server.Core.Services;
using Avalanche.Security.Server.Entities;
using Avalanche.Security.Server.ViewModels;
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
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserViewModel createUserViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<CreateUserViewModel, UserEntity>(createUserViewModel);

            var response = await _userService.CreateUserAsync(user, ERole.Common).ConfigureAwait(false);
            if(!response.Success)
            {
                return BadRequest(response.Message);
            }

            var userModel = _mapper.Map<UserEntity, UserViewModel>(response.User);
            return Ok(userModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserFilterViewModel filter)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var filterModel = _mapper.Map<UserFilterViewModel, UserFilterModel>(filter);

            var users = _mapper.Map<IList<UserEntity>, IList<UserViewModel>>(await _userService.GetUsers(filterModel).ConfigureAwait(false));

            return Ok(users);
        }
    }
}
