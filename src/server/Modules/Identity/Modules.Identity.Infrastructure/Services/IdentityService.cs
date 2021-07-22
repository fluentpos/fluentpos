using FluentPOS.Modules.Identity.Core.Abstractions;
using FluentPOS.Modules.Identity.Core.Entities;
using FluentPOS.Modules.Identity.Core.Exceptions;
using FluentPOS.Shared.Core.Constants;
using FluentPOS.Shared.Core.Interfaces.Services;
using FluentPOS.Shared.Core.Settings;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Identity;
using FluentPOS.Shared.DTOs.Mails;
using FluentPOS.Shared.DTOs.Sms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FluentPOS.Modules.Identity.Core.Features.Users.Events;

namespace FluentPOS.Modules.Identity.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<FluentUser> _userManager;
        private readonly IJobService _jobService;
        private readonly IMailService _mailService;
        private readonly MailSettings _mailSettings;
        private readonly SmsSettings _smsSettings;
        private readonly ISmsService _smsService;
        private readonly IStringLocalizer<IdentityService> _localizer;

        public IdentityService(
            UserManager<FluentUser> userManager,
            IJobService jobService,
            IMailService mailService,
            IOptions<MailSettings> mailSettings,
            IOptions<SmsSettings> smsSettings,
            ISmsService smsService,
            IStringLocalizer<IdentityService> localizer)
        {
            _userManager = userManager;
            _jobService = jobService;
            _mailService = mailService;
            _mailSettings = mailSettings.Value;
            _smsSettings = smsSettings.Value;
            _smsService = smsService;
            _localizer = localizer;
        }

        public async Task<IResult> RegisterAsync(RegisterRequest request, string origin)
        {
            var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
            if (userWithSameUserName != null)
            {
                throw new IdentityException(string.Format(_localizer["Username {0} is already taken."], request.UserName));
            }
            var user = new FluentUser
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true
            };
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                var userWithSamePhoneNumber = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber);
                if (userWithSamePhoneNumber != null)
                {
                    throw new IdentityException(string.Format(_localizer["Phone number {0} is already registered."], request.PhoneNumber));
                }
            }
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail == null)
            {
                user.AddDomainEvent(new UserRegisteredEvent(user));
                var result = await _userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    try
                    {
                        await _userManager.AddToRoleAsync(user, RoleConstants.Staff);
                    }
                    catch
                    {
                    }

                    if (!_mailSettings.EnableVerification && !_smsSettings.EnableVerification)
                    {
                        return await Result<Guid>.SuccessAsync(user.Id, message: string.Format(_localizer["User {0} Registered."], user.UserName));
                    }

                    var messages = new List<string> { string.Format(_localizer["User {0} Registered."], user.UserName) };
                    if (_mailSettings.EnableVerification)
                    {
                        // send verification email
                        var emailVerificationUri = await GetEmailVerificationUri(user, origin);
                        var mailRequest = new MailRequest
                        {
                            From = "mail@codewithmukesh.com",
                            To = user.Email,
                            Body = string.Format(_localizer["Please confirm your FluentPOS account by <a href='{0}'>clicking here</a>."], emailVerificationUri),
                            Subject = _localizer["Confirm Registration"]
                        };
                        _jobService.Enqueue(() => _mailService.SendAsync(mailRequest));

                        messages.Add(_localizer["Please check your Mailbox to verify!"]);
                    }

                    if (_smsSettings.EnableVerification)
                    {
                        // send verification sms
                        var mobilePhoneVerificationCode = await GetMobilePhoneVerificationCode(user);
                        var smsRequest = new SmsRequest
                        {
                            Number = user.PhoneNumber,
                            Message = string.Format(_localizer["Please confirm your FluentPOS account by this code: {0}"], mobilePhoneVerificationCode)
                        };
                        _jobService.Enqueue(() => _smsService.SendAsync(smsRequest));

                        messages.Add(_localizer["Please check your Phone for code in SMS to verify!"]);
                    }

                    return await Result<Guid>.SuccessAsync(user.Id, messages: messages);
                }
                else
                {
                    throw new IdentityException(_localizer["Validation Errors Occurred."], result.Errors.Select(a => _localizer[a.Description].ToString()).ToList());
                }
            }
            else
            {
                throw new IdentityException(string.Format(_localizer["Email {0} is already registered."], request.Email));
            }
        }

        private async Task<string> GetEmailVerificationUri(FluentUser user, string origin)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "api/identity/confirm-email/";
            var endpointUri = new Uri(string.Concat($"{origin}/", route));
            var verificationUri = QueryHelpers.AddQueryString(endpointUri.ToString(), "userId", user.Id.ToString());
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);
            return verificationUri;
        }

        private async Task<string> GetMobilePhoneVerificationCode(FluentUser user)
        {
            return await _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
        }

        public async Task<IResult<Guid>> ConfirmEmailAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new IdentityException(_localizer["An error occurred while confirming E-Mail."]);
            }
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                if (user.PhoneNumberConfirmed || !_smsSettings.EnableVerification)
                {
                    return await Result<Guid>.SuccessAsync(user.Id, string.Format(_localizer["Account Confirmed for E-Mail {0}. You can now use the /api/identity/token endpoint to generate JWT."], user.Email));
                }
                else
                {
                    return await Result<Guid>.SuccessAsync(user.Id, string.Format(_localizer["Account Confirmed for E-Mail {0}. You should confirm your Phone Number before using the /api/identity/token endpoint to generate JWT."], user.Email));
                }
            }
            else
            {
                throw new IdentityException(string.Format(_localizer["An error occurred while confirming {0}"], user.Email));
            }
        }

        public async Task<IResult<Guid>> ConfirmPhoneNumberAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new IdentityException(_localizer["An error occurred while confirming Mobile Phone."]);
            }
            var result = await _userManager.ChangePhoneNumberAsync(user, user.PhoneNumber, code);
            if (result.Succeeded)
            {
                if (user.EmailConfirmed)
                {
                    return await Result<Guid>.SuccessAsync(user.Id, string.Format(_localizer["Account Confirmed for Phone Number {0}. You can now use the /api/identity/token endpoint to generate JWT."], user.PhoneNumber));
                }
                else
                {
                    return await Result<Guid>.SuccessAsync(user.Id, string.Format(_localizer["Account Confirmed for Phone Number {0}. You should confirm your E-mail before using the /api/identity/token endpoint to generate JWT."], user.PhoneNumber));
                }
            }
            else
            {
                throw new IdentityException(string.Format(_localizer["An error occurred while confirming {0}"], user.PhoneNumber));
            }
        }

        public async Task<IResult> ForgotPasswordAsync(ForgotPasswordRequest request, string origin)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                throw new IdentityException(_localizer["An Error has occurred!"]);
            }
            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "account/reset-password";
            var endpointUri = new Uri(string.Concat($"{origin}/", route));
            var passwordResetUrl = QueryHelpers.AddQueryString(endpointUri.ToString(), "Token", code);
            var mailRequest = new MailRequest
            {
                Body = string.Format(_localizer["Please reset your password by <a href='{0}>clicking here</a>."], HtmlEncoder.Default.Encode(passwordResetUrl)),
                Subject = _localizer["Reset Password"],
                To = request.Email
            };
            //BackgroundJob.Enqueue(() => _mailService.SendAsync(mailRequest));
            return await Result.SuccessAsync(_localizer["Password Reset Mail has been sent to your authorized Email."]);
        }

        public async Task<IResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                throw new IdentityException(_localizer["An Error has occurred!"]);
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);
            if (result.Succeeded)
            {
                return await Result.SuccessAsync(_localizer["Password Reset Successful!"]);
            }
            else
            {
                throw new IdentityException(_localizer["An Error has occurred!"]);
            }
        }
    }
}