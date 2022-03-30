using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Managers.AutoLogin;
using Avalanche.Api.ViewModels;
using Avalanche.Api.ViewModels.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Avalanche.Api.Controllers.V1
{
    [Route("[controller]")]
    [ApiController]
    public class AutoLoginController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAutoLoginManager _autoLoginManager;

        public AutoLoginController(IMapper mapper, IAutoLoginManager autoLoginManager)
        {
            _autoLoginManager = autoLoginManager;
            _mapper = mapper;
        }

        [Route("AutoLoginEnabled")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AutoLoginEnabled()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _autoLoginManager.AutoLoginEnabled().ConfigureAwait(false);

            return Ok(response);
        }


        [Route("AutoLogin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AccessTokenViewModel))]
        public async Task<IActionResult> AutoLoginTokenAsync()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _autoLoginManager.CreateAccessTokenAsync().ConfigureAwait(false);
            if (!response.Success)
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

            var response = await _autoLoginManager.RefreshTokenAsync(refreshTokenResource.Token).ConfigureAwait(false);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }

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

            _autoLoginManager.RevokeRefreshToken(revokeTokenResource.Token);
            return NoContent();
        }
    }
}
