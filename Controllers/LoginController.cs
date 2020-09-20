using System;
using System.Linq;
using System.Web.Mvc;
using test.Models;

namespace test.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }
        [_SessionControl]
        [HttpPost]
        public ActionResult Index(string name, string pass, login login)
        {

            using (gtc_stokEntities2 db = new gtc_stokEntities2())
            {

                var userDetail = db.login.Where(x => x.name == name).FirstOrDefault();
                if (userDetail == null)
                {
                    login.lgnerror = "Kullanıcı adını boş bırakmayın";
                    return View("Index", login);
                }
                else
                {
                    var hashCode = userDetail.VCode;
                    ////Password Hasing Process Call Helper Class Method    
                    var encodingPasswordString = Helper.EncodePassword(pass, hashCode);
                    var query = db.login.Where(x => x.name == name && x.pass.Equals(encodingPasswordString)).FirstOrDefault();
                    if (query != null)
                    {
                        Session["status"] = userDetail.status;
                        Session["statusname"] = userDetail.yetki.stat;
                        Session["username"] = userDetail.name;
                        Session["UserId"] = userDetail.id;
                        return RedirectToAction("control");
                    }
                    else
                    {
                        login.lgnerror = "Kullanıcı adı ya da Şifre yanlış";
                        return View("Index", login);
                    }
                }
            }
        }
        public ActionResult Control()
        {
            return View();
        }
        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Index", "Login");
        }
    }
}