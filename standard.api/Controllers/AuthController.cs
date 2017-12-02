using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using System.Security.Principal;
using Microsoft.IdentityModel.Tokens;
using standard.api.Models;
using standard.api.Infra.Security;

namespace band_api_auth.Controllers
{

    public class ValuesController : Controller
    {
        private readonly JwtTokenOptions _tokenOptions;
        private User _usuario;


        public ValuesController(IOptions<JwtTokenOptions> jwtOptions)
        {
            _tokenOptions = jwtOptions.Value;
        }

        [AllowAnonymous]
        [Route("/token/request")]
        public async Task<IActionResult> RequestToken([FromForm]User user)
        {


            if (user == null)
                return BadRequest("Usuário ou Senha inválidos.");

            var identity = await ObterClaims(user);

            if (identity == null)
                return BadRequest("Usuário ou Senha inválidos.");


            var userClaims = new[]
            {
            new Claim(JwtRegisteredClaimNames.UniqueName, _usuario.Name.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, _usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, await _tokenOptions.JtiGenerator()),
            new Claim(ClaimTypes.Role, _usuario.Role),
            new Claim(JwtRegisteredClaimNames.Iat, _tokenOptions.ToUnixEpochDate().ToString(), ClaimValueTypes.Integer64),

            identity.FindFirst("JwtValidation")
        };

            var jwt = new JwtSecurityToken(
                issuer: _tokenOptions.Issuer,
                audience: _tokenOptions.Audience,
                claims: userClaims,
                notBefore: _tokenOptions.NotBefore,
                expires: _tokenOptions.Expiration,
                signingCredentials: _tokenOptions.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                token = encodedJwt,
                expires = _tokenOptions.Expiration,
                usuario = new
                {
                    nome = _usuario.Name,
                    role = _usuario.Role
                }
            };

            return new OkObjectResult(
                JsonConvert.SerializeObject(response));
        }

        private Task<ClaimsIdentity> ObterClaims(User user)
        {
            var usuario = Authentication.AuthenticateUserLogin(user.UserName, user.PassWord);

            if (usuario == null)
                return Task.FromResult<ClaimsIdentity>(null);

            _usuario = new User
            {
                Name = usuario.Name,
                UserName = user.UserName,
                Role = usuario.Role
            };

            return Task.FromResult(new ClaimsIdentity(
                new GenericIdentity(_usuario.Name, "Token"),
                new[] {
                new Claim(ClaimTypes.Role, _usuario.Role),
                new Claim("JwtValidation", "Usuario")
                }));
        }

        [AllowAnonymous]
        [Route("/token/validate")]
        public bool ValidarToken([FromBody] Validate validate)
        {

            var validationParameters = new TokenValidationParameters()
            {
                ValidIssuer = _tokenOptions.Issuer,
                ValidAudience = _tokenOptions.Audience,
                IssuerSigningKey = _tokenOptions.SigningKey,
                RequireExpirationTime = true
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = null;

            try
            {
                tokenHandler.ValidateToken(validate.token, validationParameters, out securityToken);
            }
            catch
            {
                return false;
            }

            return securityToken != null;
        }
    }
}
