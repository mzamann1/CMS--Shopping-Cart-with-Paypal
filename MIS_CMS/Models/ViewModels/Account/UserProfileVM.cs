﻿using MIS_CMS.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MIS_CMS.Models.ViewModels.Account
{
    public class UserProfileVM
    {
        public UserProfileVM()
        {

        }
        public UserProfileVM(UserDTO dto)
        {
            Id = dto.Id;
            FirstName = dto.FirstName;
            LastName = dto.LastName;
            EmailAddress = dto.EmailAddress;
            UserName = dto.UserName;
            Password = dto.Password;

        }
        
        [Required]
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
        [Required]
        public string UserName { get; set; }
        
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}