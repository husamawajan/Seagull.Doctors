using Seagull.Core.Data.Model;
using Seagull.Doctors.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Seagull.Core.Models
{
    public class User
    {
        public User()
        {
            fk_UserRoleMap = new List<UserRoleMap>();
            //fk_VacancyPostUser = new VacancyPost();
        }
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Mobile")]
        public string Mobile { get; set; }

        [Display(Name = "LangId")]
        public int? LangId { get; set; }

        public int? ProducerId { get; set; }
        public int? FildLogIn { get; set; }

        public bool? Activation { get; set; }

        public bool? IsDeleted { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public virtual List<UserRoleMap> fk_UserRoleMap { get; set; }

        #region Token
        public string JwtToken { get; set; }
        public DateTime? CreateDateJwtToken { get; set; }
        public DateTime? UdateDateJwtToken { get; set; }
        public string FCMToken { get; set; }
        public DateTime? CreateDateFCMToken { get; set; }
        public DateTime? UdateDateFCMToken { get; set; }


        #endregion

    }
}
