﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Seagull.Core.Data;
using Seagull.Core.Data.Interfaces;
using Seagull.Core.Data.Model;
using Seagull.Core.Models;
using Seagull.Core.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Seagull.API.APIHelper;
using Seagull.API.APIHelper.Authorization;
using static Seagull.API.APIModels.UserAPI;
using System.Security.Claims;
using Seagull.API.LocalizationApi;
using static Seagull.API.Controllers.PublicApiController;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Seagull.Core.Helper.StaticVariables;
using Seagull.Doctors.Data.Model;

namespace Seagull.API.Controllers
{
    [Route("Seagull")]
    public class AuthController : ControllerBase
    {
        public string AcceptLanguage
        {
            get
            {
                return Request.Headers["Accept-Language"].ToString();
            }
        }
        JsonStringLocalizerApi _localizer;
        private LibraryDbContext _context;
        private readonly DbSet<User> _users;
        private readonly DbSet<GustUserDevice> _gustUserDevice;
        public AuthController(LibraryDbContext context)
        {
            _localizer = new JsonStringLocalizerApi();
            _context = context;
            _users = _context.Set<User>();
            _gustUserDevice = _context.Set<GustUserDevice>();
            _users = _context.Set<User>();
        }

        [HttpPost("login")]
        public ActionResult UserLogIn([FromBody]APILogin logInUser)
        {
            APIJsonResult result = new APIJsonResult();
            APILoginView _aPILoginView = new APILoginView();
            result.data = _aPILoginView;
            result.Access = true;
            result.success = false;

            var currentUserToken = User.Claims.SingleOrDefault(x => x.Type == "UserId") != null ? User.Claims.SingleOrDefault(x => x.Type == "UserId").Value : null;
            var UserRole = User.Claims.SingleOrDefault(x => x.Type == "UserRole") != null ? User.Claims.SingleOrDefault(x => x.Type == "UserRole").Value : null;


            //if (currentUserToken == null)
            //{
            //    result.Msg.Add(_localizer.GetString("web.Session Time Out"));
            //    result.success = false;
            //    return Ok(result);

            //}
            int fildLogIn = Convert.ToInt32(_context.SystemSettings.Where(x => x.Name == "FildLogInNumber").FirstOrDefault().Value);

            if (User == null)
            {
                result.success = false;
                result.Msg.Add(_localizer.GetString("All fields must be filled"));
                return Ok(result);
            }
            else if (string.IsNullOrEmpty(logInUser.Email) || string.IsNullOrEmpty(logInUser.Password))
            {
                result.success = false;
                result.Msg.Add(_localizer.GetString("All fields must be filled"));
                return Ok(result);
            }
            using (var algorithm = MD5.Create()) //or MD5 SHA256 etc.
            {
                var hashedBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(logInUser.Password));

                logInUser.Password = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
            var user = _context.Users.Where(x => x.Email == logInUser.Email && x.IsDeleted != true && x.Password == logInUser.Password).Include(f => f.fk_UserRoleMap).ThenInclude(g => g.UserRole).ThenInclude(p => p.fk_UserRolePermMap).ThenInclude(h => h.Permission).FirstOrDefault();

            if (user == null)
            {

                result.success = false;
                result.Msg.Add(_localizer.GetString("InCorrect Credential"));
                //result.Msg.Add(_localizer.GetString("Email") + logInUser.Email);
                //result.Msg.Add(_localizer.GetString("Password") + logInUser.Password);
                return Ok(result);
            }
            if (user.FildLogIn == fildLogIn)
            {
                user.Activation = false;
                _context.Update(user);
                _context.SaveChanges();
                result.Msg.Add("Admin.YourAccountHasBeenBlocked");
                return Ok(result);
            }
            if (user.Password == logInUser.Password)
            {
                user.FildLogIn = 0;
                if (user.JwtToken != null)
                {
                    user.JwtToken = result.token = Token.GenerateToken(user.Id.ToString(), user.fk_UserRoleMap[0].UserRole.Name);
                    user.UdateDateFCMToken = DateTime.Now;
                }
                else
                {
                    user.JwtToken = result.token = Token.GenerateToken(user.Id.ToString(), user.fk_UserRoleMap[0].UserRole.Name);
                    user.CreateDateJwtToken = DateTime.Now;
                }

                if (currentUserToken != null)
                {
                    var gustUser = _gustUserDevice.Where(x => x.DeviceId == currentUserToken).FirstOrDefault();
                    if (gustUser != null)
                        _gustUserDevice.Remove(gustUser);
                }
                _context.Update(user);
                _context.SaveChanges();
            }
            else
            {
                user.FildLogIn = user.FildLogIn == null ? 1 : user.FildLogIn + 1;
                // User Type
                _context.Update(user);
                _context.SaveChanges();

                result.Msg.Add(_localizer.GetString("Admin.Your Password Incorrect Number of  attempts to log in -") + (fildLogIn - user.FildLogIn) + " -");
                return Ok(result);
            }
            result.success = true;
            UserProfile userData = new UserProfile()
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Mobile = user.Mobile,
                UserType = user.fk_UserRoleMap[0].UserRole.Name
            };
            result.data = userData;
            result.Msg.Add(_localizer.GetString("Admin.Successfully logged In"));
            return Ok(result);
        }

        [HttpPost("register")]
        public ActionResult RegisterUser([FromBody] UserRegisterApi userRegister)
        {
            _localizer.HeaderLanguage = AcceptLanguage;
            APIJsonResult result = new APIJsonResult();
            UserProfile userDate = new UserProfile();
            result.Access = true;

            if (string.IsNullOrEmpty(userRegister.Email) || string.IsNullOrEmpty(userRegister.Name)
                || string.IsNullOrEmpty(userRegister.Mobile))
            {
                result.success = false;
                result.Msg.Add("Admin.All fields must be filled");
                result.data = new APILoginView();
                return Ok(result);
            }
            //#region Validate Phone Number
            //Regex pattern = new Regex(RegexStrings.MobileRegex);
            //if (!pattern.IsMatch(userRegister.Mobile))
            //{
            //    result.success = false;
            //    result.Msg.Add(_localizer.GetString("Admin.Invalid Mobile number"));
            //    result.data = null;
            //    return Ok(result);
            //}
            //#endregion
            #region Validate Email 
            Regex emailPattern = new Regex(RegexStrings.EmailRegex);
            if (!emailPattern.IsMatch(userRegister.Email))
            {
                result.success = false;
                result.Msg.Add(_localizer.GetString("Admin.Invalid Email Address "));
                result.data = null;
                return Ok(result);
            }
            #endregion
            result.success = false;
            var user = new User();
            var Existuser = _context.Users.Where(x => x.Email == userRegister.Email && x.IsDeleted != true).Include(f => f.fk_UserRoleMap).ThenInclude(g => g.UserRole).ThenInclude(p => p.fk_UserRolePermMap).ThenInclude(h => h.Permission).FirstOrDefault();
            if (Existuser == null)
            {
                using (var algorithm = MD5.Create()) //or MD5 SHA256 etc.
                {
                    var hashedBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(userRegister.Password));

                    userRegister.Password = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                }
                using (var algorithm = MD5.Create()) //or MD5 SHA256 etc.
                {
                    var hashedBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(userRegister.ConfirmPassword));

                    userRegister.ConfirmPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                }

                if (userRegister.Password.Equals(userRegister.ConfirmPassword))
                {
                    user = new User()
                    {
                        Email = userRegister.Email,
                        Mobile = userRegister.Mobile,
                        Name = userRegister.Name,
                        Password = userRegister.Password,
                        //JwtToken = result.token = Token.GenerateToken(user.Id.ToString()),
                    };
                    //user.Activation = false;
                    _context.Users.Add(user);
                    _context.SaveChanges();
                    user.fk_UserRoleMap.Add(new UserRoleMap()
                    {
                        UserId = user.Id,
                        User = user,
                        UserRoleId = 7,
                        UserRole = _context.UserRoles.Where(x => x.Id == 7).FirstOrDefault()
                    });

                    _context.Users.Update(user);
                    _context.SaveChanges();
                    userDate.Id = user.Id;
                    userDate.Email = user.Email;
                    userDate.Mobile = user.Mobile;
                    userDate.Name = user.Name;
                    // result.Msg.Add("Admin.Wait for activation from Admin");
                    result.Msg.Add("Admin.Successfully Registeration ");

                }
                else
                {
                    result.success = false;
                    result.Msg.Add("Admin.User Password Not Match");
                    result.data = new APILoginView();
                    return Ok(result);
                }

            }
            else
            {
                result.success = false;
                result.Msg.Add("Admin.duplicate email address");
                result.data = new APILoginView();
                return Ok(result);
            }
            result.success = true;
            result.data = userDate;
            return Ok(result);
        }

        [HttpPost("logout")]
        public ActionResult LogOut(Devices devices)
        {

            APIJsonResult result = new APIJsonResult();
            result.Access = true;
            result.success = true;
            result.data = new UserProfile() { Email = "", Id = -1, Mobile = "", Name = "" };

            var currentUserToken = User.Claims.SingleOrDefault(x => x.Type == "UserRole") != null
            ? User.Claims.SingleOrDefault(x => x.Type == "UserRole").Value : null;


            var gustUserDevice = _gustUserDevice.Where(x => x.DeviceId == devices.deviceId).FirstOrDefault();
            if (gustUserDevice == null)
            {
                _gustUserDevice.Add(new GustUserDevice()
                {
                    DeviceId = devices.deviceId,
                    JwtToken = result.token = Token.GenerateToken(devices.deviceId, currentUserToken),
                    CreatedDate = DateTime.Now,
                });
                _context.SaveChanges();
            }
            else
            {
                gustUserDevice.UpdatedDate = DateTime.Now;
                gustUserDevice.JwtToken = result.token = Token.GenerateToken(devices.deviceId, currentUserToken);
                _gustUserDevice.Update(gustUserDevice);
                _context.SaveChanges();
            }

            return Ok(result);
        }
    }
}