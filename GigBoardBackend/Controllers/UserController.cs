using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GigBoardBackend.Services;
using GigBoardBackend.Models;
using GigBoardBackend.Data;
using System.Security.Claims;

namespace GigBoardBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            try
            {
                var response = await _userService.CreateUserAsync(user);
                return Ok(response);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            try
            {
                var response = await _userService.LoginUserAsync(user);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] User user)
        {
            try
            {
                var result = await _userService.UpdateUserAsync(user);
                return Ok(result);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [Authorize]
        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                var result = await _userService.GetUserByUsernameAsync(username);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [Authorize]
        [HttpGet("has-data")]
        public async Task<IActionResult> GetUserHasData()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("User claim is invalid");
            }

            var hasData = await _userService.GetUserHasDataAsync(userId);
            return Ok(hasData);
        }
    }
}