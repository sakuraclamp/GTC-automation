using MongoDB.Driver;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using test.Models;
namespace test.Controllers
{
    public class AdminControlController : Controller
    {
        // GET: AdminControl
        gtc_stokEntities2 db = new gtc_stokEntities2();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Kisilistele()
        {
            var liste = db.login.OrderByDescending(x => x.id).ToList();
            return View(liste);
        }
        public ActionResult Profil(string name)
        {
            return View(db.login.Where(x => x.name == name).FirstOrDefault());
        }
        [HttpPost]
        public ActionResult Profil(login login)
        {
            try
            {
                var keyNew = Helper.GeneratePassword(10);
                var password = Helper.EncodePassword(login.pass, keyNew);
                login.VCode = keyNew;
                login.pass = password;
                db.Entry(login).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        public ActionResult Rolekle()
        {
            yetki yetki = new yetki();
            return View(yetki);
        }
        [HttpPost]
        public ActionResult Rolekle(yetki yetki)
        {
            if (db.yetki.Any(x => x.stat == yetki.stat))
            {
                ViewBag.uyari = "Rol durumu bulunmakta";
                return View("Rolekle", yetki);
            }
            else if (yetki.stat == null)
            {
                ViewBag.uyari = "Boş bırakmayın";
                return View();
            }
            else
            {
                db.yetki.Add(yetki);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        public ActionResult Guvenliklog()
        {
            return View(db.guvenlikkontrol.OrderByDescending(x => x.id).ToList());
        }
    }
}