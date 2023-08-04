using AppdevBookShop.Contanst;
using AppdevBookShop.Services.IServices;
using AppdevBookShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppdevBookShop.Areas.Authenticated.Controllers;

[Area(SD.Authenticated_Area)]
[Authorize(Roles = SD.Admin_Role)]
public class UsersManagementController : BaseController
{
    private readonly IUserServices _userServices;

    public UsersManagementController(IUserServices userServices)
    {
        _userServices = userServices;
    }

    // GET
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // lấy id của người đăng nhập hiện tại
        var currentUserId = GetCurrentUserId();
        var userList = await _userServices.GetAllUser(currentUserId);

        return View(userList);
    }

    [HttpGet]
    public async Task<IActionResult> LockUnLock(string id)
    {
        await _userServices.LockUnlock(GetCurrentUserId(), id);
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> Update(String id)
    {
        if (id != null)
        {
            var userVm = await _userServices.GetUserUpdate(id);
            return View(userVm);
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Update(UserVM userVm)
    {
        // validate dữ liệu
        if (userVm.User.Id != String.Empty 
            && userVm.User.PhoneNumber != null 
            && userVm.User.FullName != string.Empty 
            && userVm.User.Role != String.Empty)
        {
            await _userServices.Update(userVm);
            return RedirectToAction(nameof(Index));
        }
        
        // trường hợp một trong các trường trên bị lỗi
        userVm.Rolelist = _userServices.GetRoleDropDown();
        return View(userVm);
    }


    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        var isSuccessfully = await _userServices.Delete(id);
        if (!isSuccessfully)
        {
            return NotFound();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string id)
    {
        var user = await _userServices.GetUserById(id);

        if (user == null)
        {
            return View();
        }

        ConfirmEmailVM confirmEmailVm = new ConfirmEmailVM()
        {
            Email = user.Email
        };

        return View(confirmEmailVm);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailVM confirmEmailVm)
    {
        if (ModelState.IsValid)
        {
            var (email, token) = await _userServices.ConfirmEmail(confirmEmailVm);

            return RedirectToAction("ResetPassword", "UsersManagement", 
                new { token = token, email = email });
        }

        return View(confirmEmailVm);
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string token, string email)
    {
        if (token == null || email == null)
        {
            ModelState.AddModelError("", "Invalid password reset token");
        }

        ResetPasswordViewModel resetPasswordViewModel = new ResetPasswordViewModel()
        {
            Email = email,
            Token = token
        };
        return View(resetPasswordViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
    {
        if (ModelState.IsValid)
        {
            var isSuccessFully = await _userServices.ResetPassword(resetPasswordViewModel);
            if (isSuccessFully)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        return View(resetPasswordViewModel);
    }
}