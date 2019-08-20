﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Seagull.API.APIModels
{
    public class UserAPI
    {
        public class APILogin
        {
            //public string Id { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }    
        }
        public class APILoginView
        {
            public APILoginView()
            {
                Email = string.Empty;
            }
            public int Id { get; set; }
            public string Email { get; set; }
        }

        public class UserProfile
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Mobile { get; set; }
            //public string Address { get; set; }
            public string Email { get; set; }
            public string UserType { get; set; }

        }
        public class UpdateUserProfile
        {
            //public int Id { get; set; }
            public string Name { get; set; }
            public string Mobile { get; set; }
            //public string Address { get; set; }
            public string Email { get; set; }
        }
        public class UserRegisterApi
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public bool AcceptCondition { get; set; }
            public string Mobile { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
        }
    }
}
