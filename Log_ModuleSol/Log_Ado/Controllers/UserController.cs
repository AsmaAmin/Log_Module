using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Log_Ado.Models;

namespace Log_Ado.Controllers
{
    public class UserController : Controller
    {
       //Registration Action
       [HttpGet]
       public ActionResult Registration()
        {
            return View();
        }
        // Registration Post Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVarified, ActivationCode")]User user)
        {
            bool Status = false;
            string message = "";
            //Model Validation
            if (ModelState.IsValid)
            {
                #region Email is already exist
                var isExist = IsEmailExist(user.EmailId);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(user);
                }

                #endregion

                #region Generate Activate Code
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region Password hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);
                #endregion

                user.IsEmailVarified = false;

                #region Save Data to Database
                using (Log_ModuledbEntities dc = new Log_ModuledbEntities())
                {
                    dc.Users.Add(user);
                    dc.SaveChanges();


                    SendVerificationLinkEmail(user.EmailId, user.ActivationCode.ToString());
                    message = "Regisration successfully done" + user.EmailId;
                    Status = true;
                }
                #endregion
            }
            else
            {
                message = "Invalid Rquest";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }
        // verify Email
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool status = false;
            using (Log_ModuledbEntities dc = new Log_ModuledbEntities())
            {
                dc.Configuration.ValidateOnSaveEnabled = false;

                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();

                if (v != null)
                {
                    v.IsEmailVarified = true;
                    dc.SaveChanges();
                    status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";

                }
                ViewBag.Status = status;
                return View();
            }
        }
      //Login
      [HttpGet]
      public ActionResult Login()
        {
            return View();
        }
        // Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string ReturnUrl="")
        {
            string message = "";
            using(Log_ModuledbEntities dc = new Log_ModuledbEntities())
            {
                var v = dc.Users.Where(a => a.EmailId == login.EmailId).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(Crypto.Hash(login.Password), v.Password) == 0)
                    {
                        int timeout = login.Rememberme ? 525600 : 20; // 525600 min = 1year;
                        var ticket = new FormsAuthenticationTicket(login.EmailId, login.Rememberme, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookis = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookis.Expires = DateTime.Now.AddMinutes(timeout);
                        cookis.HttpOnly = true;
                        Response.Cookies.Add(cookis);


                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index, Home");
                        }
                    }

                    else
                    {
                        message = "Invalid credential Provided.";
                    }
                }
            }
                return View();
        }
        //Logout

        [HttpPost]
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }
        [NonAction]
        public bool IsEmailExist(string emailId)
        {
            using (Log_ModuledbEntities dc = new Log_ModuledbEntities())
            {
                var v = dc.Users.Where(m => m.EmailId == emailId).FirstOrDefault();
                return v != null;
                    
            }
        }

        [NonAction]
        public void SendVerificationLinkEmail(string EmailId, string activationCode)
        {
            var verifyUrl = "/User/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl); 
           

            var fromEmail = new MailAddress("asmaakter9067@gmail.com", "Log Module");
            var toEmail = new MailAddress(EmailId);
            var fromEmailPassword = "2017@asma@39";
            string subject = "Your account is successfully created";

            string body = "<br/><br/>We are excited to tell you that your Dotnet Account is" +
                "successfully created. Please click on the below link  to verify your account" +
                "<br/><br/><a href='" + link + "'>" + link + "</a>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)

            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);

        }

        //[NonAction]
        //public void SendVerificationLinkEmail(string emailId, string activationCode)
        //{
        //    var verifyUrl = "/User/VerifyAccount/" + activationCode;
        //    var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

        //    var fromEmail = new MailAddress("asmaakter9067@gmail.com", "Dotnet Awesome");
        //    var toEmail = new MailAddress(emailId);
        //    var fromEmailPassword = "2017@asma@39"; // Replace with actual password
        //    string subject = "Your account is successfully created!";

        //    string body = "<br/><br/>We are excited to tell you that your Dotnet Awesome account is" +
        //        " successfully created. Please click on the below link to verify your account" +
        //        " <br/><br/><a href='" + link + "'>" + link + "</a> ";

        //    var smtp = new SmtpClient
        //    {
        //        Host = "smtp.gmail.com",
        //        Port = 587,
        //        EnableSsl = true,
        //        DeliveryMethod = SmtpDeliveryMethod.Network,
        //        UseDefaultCredentials = false,
        //        Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
        //    };

        //    using (var message = new MailMessage(fromEmail, toEmail)
        //    {
        //        Subject = subject,
        //        Body = body,
        //        IsBodyHtml = true
        //    })
        //        smtp.Send(message);
        //}
    }
}