using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Managers;
using Avalanche.Api.ViewModels;
using Avalanche.Api.ViewModels.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAuthenticationManager _authenticationManager;

        public AuthenticationController(IMapper mapper, IAuthenticationManager authenticationService)
        {
            _authenticationManager = authenticationService;
            _mapper = mapper;
        }

        [Route("token")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AccessTokenViewModel))]
        public async Task<IActionResult> LoginAsync([FromBody] UserCredentialsViewModel userCredentials)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authenticationManager.CreateAccessTokenAsync(userCredentials.UserName, userCredentials.Password);
            if(!response.Success)
            {
                return BadRequest(response.Message);
            }

            var accessTokenResource = _mapper.Map<AccessToken, AccessTokenViewModel>(response.Token);
            return Ok(accessTokenResource);
        }

        [Route("token")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AccessTokenViewModel))]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenViewModel refreshTokenResource)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authenticationManager.RefreshTokenAsync(refreshTokenResource.Token, refreshTokenResource.UserName);
            if(!response.Success)
            {
                return BadRequest(response.Message);
            }

            //TODO: Check the location of this mapping
            var tokenResource = _mapper.Map<AccessToken, AccessTokenViewModel>(response.Token);
            return Ok(tokenResource);
        }

        [Route("token")]
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult RevokeToken([FromBody] RevokeTokenViewModel revokeTokenResource)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _authenticationManager.RevokeRefreshToken(revokeTokenResource.Token);
            return NoContent();
        }
    }
}