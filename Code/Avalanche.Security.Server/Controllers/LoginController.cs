using AutoMapper;

using Avalanche.Security.Server.ViewModels;
using Avalanche.Security.Server.Core.Security.Tokens;
using Avalanche.Security.Server.Core.Services;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace Avalanche.Security.Server.Controllers
{
    //TODO: Review this, is a little bit different to the API controllers but chaange this in this moment can affect the demo
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAuthenticationManager _authenticationManager;

        public AuthController(IMapper mapper, IAuthenticationManager authenticationService)
        {
            _authenticationManager = authenticationService;
            _mapper = mapper;
        }

        [Route("/api/login")]
        [HttpPost]
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

            var accessTokenResource = _mapper.Map<AccessToken, AccessTokenResource>(response.Token);
            return Ok(accessTokenResource);
        }

        [Route("/api/token/refresh")]
        [HttpPost]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenViewModel refreshTokenResource)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authenticationManager.RefreshTokenAsync(refreshTokenResource.Token, refreshTokenResource.UserEmail);
            if(!response.Success)
            {
                return BadRequest(response.Message);
            }

            //TODO: Check the location of this mapping
            var tokenResource = _mapper.Map<AccessToken, AccessTokenResource>(response.Token);
            return Ok(tokenResource);
        }

        [Route("/api/token/revoke")]
        [HttpPost]
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
