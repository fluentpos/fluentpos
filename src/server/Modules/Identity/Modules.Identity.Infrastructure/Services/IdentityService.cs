using AutoMapper;
using FluentPOS.Modules.Identity.Core.Abstractions;
using FluentPOS.Modules.Identity.Core.Entities;
using FluentPOS.Modules.Identity.Core.Enums;
using FluentPOS.Modules.Identity.Core.Exceptions;
using FluentPOS.Shared.Application.Interfaces.Services;
using FluentPOS.Shared.Application.Wrapper;
using FluentPOS.Shared.DTOs.Identity;
using FluentPOS.Shared.DTOs.Mails;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace FluentPOS.Modules.Identity.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ExtendedIdentityUser> _userManager;
        private readonly RoleManager<ExtendedIdentityRole> _roleManager;
        private readonly IJobService _jobService;
        private readonly IMailService _mailService;

        public IdentityService(
            UserManager<ExtendedIdentityUser> userManager,
            IMapper mapper,
            RoleManager<ExtendedIdentityRole> roleManager, IJobService jobService, IMailService mailService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
            _jobService = jobService;
            _mailService = mailService;
        }

        private readonly IMapper _mapper;

        public async Task<Result<List<UserResponse>>> GetAllAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = _mapper.Map<List<UserResponse>>(users);
            return await Result<List<UserResponse>>.SuccessAsync(result);
        }

        public async Task<IResult> RegisterAsync(RegisterRequest request, string origin)
        {
            var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
            if (userWithSameUserName != null)
            {
                throw new IdentityException($"Username '{request.UserName}' is already taken.");
            }
            var user = new ExtendedIdentityUser
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true
            };
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail == null)
            {
                var result = await _userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    try
                    {
                        await _userManager.AddToRoleAsync(user, Roles.Staff.ToString());
                    }
                    catch
                    {
                    }

                    var verificationUri = await SendVerificationEmail(user, origin);
                    _jobService.Enqueue(() => _mailService.SendAsync(new MailRequest() { From = "mail@codewithmukesh.com", To = user.Email, Body = $"Please confirm your account by <a href='{verificationUri}'>clicking here</a>.", Subject = "Confirm Registration" }));
                    return await Result<int>.SuccessAsync(user.Id, message: $"User Registered. Please check your Mailbox to verify!");
                }
                else
                {
                    throw new IdentityException("Validation Errors Occurred.", result.Errors.Select(a => a.Description).ToList());
                }
            }
            else
            {
                throw new IdentityException($"Email {request.Email } is already registered.");
            }
        }

        private async Task<string> SendVerificationEmail(ExtendedIdentityUser user, string origin)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "api/identity/confirm-email/";
            var endpointUri = new Uri(string.Concat($"{origin}/", route));
            var verificationUri = QueryHelpers.AddQueryString(endpointUri.ToString(), "userId", user.Id.ToString());
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);
            return verificationUri;
        }

        public async Task<IResult<UserResponse>> GetAsync(int userId)
        {
            var user = await _userManager.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            var result = _mapper.Map<UserResponse>(user);
            return await Result<UserResponse>.SuccessAsync(result);
        }

        public async Task<IResult<UserRolesResponse>> GetRolesAsync(string userId)
        {
            var viewModel = new List<UserRoleModel>();
            var user = await _userManager.FindByIdAsync(userId);
            foreach (var role in _roleManager.Roles)
            {
                var userRolesViewModel = new UserRoleModel
                {
                    RoleName = role.Name
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                viewModel.Add(userRolesViewModel);
            }
            var result = new UserRolesResponse { UserRoles = viewModel };
            return await Result<UserRolesResponse>.SuccessAsync(result);
        }

        public async Task<IResult<int>> ConfirmEmailAsync(int userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return await Result<int>.SuccessAsync(user.Id, message: $"Account Confirmed for {user.Email}.You can now use the /api/identity/token endpoint to generate JWT.");
            }
            else
            {
                throw new IdentityException($"An error occurred while confirming user.Email.");
            }
        }

        public async Task<IResult> ForgotPasswordAsync(string emailId, string origin)
        {
            var user = await _userManager.FindByEmailAsync(emailId);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                throw new IdentityException("An Error has occurred!");
            }
            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "account/reset-password";
            var endpointUri = new Uri(string.Concat($"{origin}/", route));
            var passwordResetURL = QueryHelpers.AddQueryString(endpointUri.ToString(), "Token", code);
            var request = new MailRequest
            {
                Body = $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(passwordResetURL)}'>clicking here.</a>.",
                Subject = "Reset Password",
                To = emailId
            };
            //BackgroundJob.Enqueue(() => _mailService.SendAsync(request));
            return await Result.SuccessAsync("Password Reset Mail has been sent to your authorized EmailId.");
        }

        public async Task<IResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                throw new IdentityException("An Error has occurred!");
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);
            if (result.Succeeded)
            {
                return await Result.SuccessAsync("Password Reset Successful!");
            }
            else
            {
                throw new IdentityException("An Error has occurred!");
            }
        }
    }
}