using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public IndexController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<string> GetAsync()
        {
            AppUser user = new AppUser();

            user.UserName = "jakub";
            user.FirstName = "Test";
            user.Email = "aaa@wp.pl";

            var result = await _userManager.CreateAsync(user, "testowooo");

            return await Task.FromResult("I'm alive");
        }
    }
}