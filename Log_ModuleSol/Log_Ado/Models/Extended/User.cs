using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Log_Ado.Models
{
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
        public string ConfirmPassword { get; set; }
    }

    public class UserMetadata
    {
        [Display(Name = "First Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "First name required")]
        public string FirstName { get; set; }


        [Display(Name = "Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last Name Required")]
        public string LastName { get; set; }

        [Display(Name = "Email ID")]
        [Required(AllowEmptyStrings = false, ErrorMessage =" Email ID is required")]
        [DataType(DataType.EmailAddress)]
        public string EmailId { get; set; }


        [Display(Name ="Date of Birth")]
        [DisplayFormat(ApplyFormatInEditMode =true, DataFormatString ="(0:MM/dd/yyyy)")]
        [DataType(DataType.Date)]
        public DateTime DateofBirth { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage =" Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage ="Minimum 6 characters required")]
        public string Password { get; set; }


        [Display(Name ="Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage ="Confirm Password and Password do not match.")]
        public string ConfirmPassword { get; set; }


    }
}