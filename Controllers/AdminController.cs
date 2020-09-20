using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using test.Models;


namespace test.Controllers
{

    public class AdminController : Controller
    {
        gtc_stokEntities2 db = new gtc_stokEntities2();

        //excel döküman alma işlemi testi
        //public ActionResult Excel()
        //{
        //    using (var workbook = new XLWorkbook())
        //    {
        //        var worksheet = workbook.Worksheets.Add("Users");
        //        var rows = 1;
        //        var columnB = worksheet.Column("B");
        //        columnB.Width = 15;
        //        var columnC = worksheet.Column("C");
        //        columnC.Width = 14;
        //        var columnD = worksheet.Column("D");
        //        columnD.Width = 15;
        //        var columnE = worksheet.Column("E");
        //        columnE.Width = 25;
        //        var columnF = worksheet.Column("F");
        //        columnF.Width = 13;
        //        var columnH = worksheet.Column("H");
        //        columnH.Width = 13;
        //        var columnI = worksheet.Column("I");
        //        columnI.Width = 13;
        //        var columnJ = worksheet.Column("J");
        //        columnJ.Width = 13;
        //        worksheet.Cell(rows, 1).Value = "Id";
        //        worksheet.Cell(rows, 2).Value = "Log Kayıt Tarihi";
        //        worksheet.Cell(rows, 3).Value = "Stok Giriş Tarihi";
        //        worksheet.Cell(rows, 4).Value = "Kullanıcı";
        //        worksheet.Cell(rows, 5).Value = "Tedarikçi Firma";
        //        worksheet.Cell(rows, 6).Value = "Türü";
        //        worksheet.Cell(rows, 7).Value = "Miktar";
        //        worksheet.Cell(rows, 8).Value = "Birim";
        //        worksheet.Cell(rows, 9).Value = "Malzeme";
        //        worksheet.Cell(rows, 10).Value = "Durum";

        //        var liste = db.adminkontrol.ToList();
        //        ViewBag.list = liste;
        //        foreach (var user in ViewBag.list)
        //        {
        //            rows++;
        //            worksheet.Cell(rows, 1).Value = user.id;
        //            worksheet.Cell(rows, 2).Value = user.kayittarih;
        //            worksheet.Cell(rows, 3).Value = user.stok.tarih;
        //            worksheet.Cell(rows, 4).Value = user.login.name;
        //            worksheet.Cell(rows, 5).Value = user.tedarikci.firma;
        //            worksheet.Cell(rows, 6).Value = user.tur1.turu;
        //            worksheet.Cell(rows, 7).Value = user.miktar;
        //            worksheet.Cell(rows, 8).Value = user.birim1.birim1;
        //            worksheet.Cell(rows, 9).Value = user.malzemelist.malzeme;
        //            if(user.giren==1)
        //            {
        //                worksheet.Cell(rows, 10).Value = "Ürün Girdi";
        //            }
        //            else if(user.giren == 2)
        //            {
        //                worksheet.Cell(rows, 10).Value = "Güncellendi";
        //            }
        //            else
        //            {
        //                worksheet.Cell(rows, 10).Value = "Ürün Kaldı";
        //            }
        //        }

        //        using (var stream = new MemoryStream())
        //        {


        //            workbook.SaveAs(stream);
        //            var content = stream.ToArray();

        //            return File(
        //                content,
        //                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //                "StokLog.xlsx");
        //        }
        //    }
        //}

        public ActionResult Kisiekle()
        {
            ViewBag.yetki = new SelectList(db.yetki.Where(x => x.id > 0).OrderBy(x => x.id), "id", "stat");
            login user = new login();
            return View(user);
        }
        [_SessionControl]
        [HttpPost]
        public ActionResult Kisiekle(login user, int yetki)
        {
            ViewBag.yetki = new SelectList(db.yetki.OrderBy(x => x.stat), "id", "stat");
            if (db.login.Any(x => x.name == user.name))
            {
                ViewBag.kullanıvar = "Kullanıcı bulunmakta";
                return View("Kisiekle", user);
            }
            else if (user.name == null)
            {
                ViewBag.kullanıvar = "Kullanıcı adını boş bırakmayın";
                return View("Kisiekle", user);
            }
            else if (user.pass == null)
            {
                ViewBag.kullanıvar = "Şifre alanını boş bırakmayın";
                return View("Kisiekle", user);
            }
            else if (user.pass.Length < 4)
            {
                ViewBag.kullanıvar = "Daha uzun bir şifre girin";
                return View("Kisiekle", user);
            }
            else
            {
                var keyNew = Helper.GeneratePassword(10);
                var password = Helper.EncodePassword(user.pass, keyNew);
                user.VCode = keyNew;
                user.pass = password;
                user.status = yetki;
                db.login.Add(user);
                db.SaveChanges();
            }

            ModelState.Clear();
            return RedirectToAction("Kisilistele", "AdminControl");
        }

        //parçalı olarak tarihe ait ay'ı çeker
        //var ay = item.kayittarih.Date.Month;

        public ActionResult Index()
        {
            double[] giren = new double[13];
            double[] kalan = new double[13];
            var a = DateTime.Now.Year;
            //Jchat kısmında yer alan dropdown'a yıllara ait verilerin gönderilmesini sağlar
            ViewBag.ylr = new SelectList(new SelectList(
                        Enumerable.Range(2020, 50)
                            .Select(r => new
                            {
                                Text = new DateTime(r, 1, 1).ToString("yyyy"),
                                Value = r.ToString()
                            }),
                        "Value", "Text", "").ToList(), "Value", "Text", a.ToString());
            for (int i = 1; i < 13; i++)
            {
                giren[i] = 0;
                kalan[i] = 0;
            }
            var adminkontrols = db.adminkontrol.OrderBy(x => x.kayittarih).ToList();
            ViewBag.dd = adminkontrols;
            foreach (var item in ViewBag.dd)
            {
                if (item.kayittarih.Date.Year == a)
                {
                    int temp = item.kayittarih.Date.Month;
                    for (int i = 1; i < 13; i++)
                    {
                        if (temp == i)
                        {
                            if (item.giren == 1)
                            {
                                giren[i] += item.girilen;
                                i = 13;
                            }
                            else if (item.giren == 0)
                            {
                                kalan[i] += item.harcanan;
                                i = 13;
                            }
                        }
                    }
                }
            }

            //Js kısmındaki Data'ya gerekli olan verilerin yollanmasını sağlar
            ViewBag.DataPoints1 = JsonConvert.SerializeObject(giren);
            ViewBag.DataPoints2 = JsonConvert.SerializeObject(kalan);
            var liste = db.stok_list();

            //var liste = db.malzemelist.OrderBy(x => x.malzeme).ToList();
            //var data = (from malzeme in db.malzemelist
            //             join stok in db.stok
            //             on malzeme.id equals stok.malzeme_id
            //             group stok by malzeme into grup
            //             select new 
            //             {
            //                 maladi = grup,
            //                 malzemetop = grup.Key,
            //                 total=grup.Sum(x=>x.miktar)
            //             }).ToList() ;
            //ViewData.ModelMetadata;

            ViewBag.kullanicilar = db.login.Count() - 1;
            ViewBag.us = db.login.Where(x => x.status == 1).Count();
            ViewBag.ms = db.login.Where(x => x.status == 2).Count();
            ViewBag.stoktaki = db.stok.Where(x => x.isdisable == 0).Count();
            ViewBag.malzemeler = db.malzemelist.Count();
            ViewBag.firmalar = db.tedarikci.Count();
            ViewBag.list = liste;
            ViewBag.mlz = new SelectList(db.malzemelist.OrderBy(x => x.malzeme), "malzeme", "malzeme");
            return View();

        }
        [HttpPost]
        public ActionResult Index(int ylr, string mlz)
        {
            var liste = db.stok_list();
            ViewBag.kullanicilar = db.login.Count() - 1;
            ViewBag.us = db.login.Where(x => x.status == 1).Count();
            ViewBag.ms = db.login.Where(x => x.status == 2).Count();
            ViewBag.stoktaki = db.stok.Where(x => x.isdisable == 0).Count();
            ViewBag.malzemeler = db.malzemelist.Count();
            ViewBag.firmalar = db.tedarikci.Count();
            ViewBag.list = liste;

            double[] giren = new double[13];
            double[] kalan = new double[13];

            for (int i = 1; i < 13; i++)
            {
                giren[i] = 0;
                kalan[i] = 0;
            }
            var adminkontrols = db.adminkontrol.OrderBy(x => x.kayittarih).ToList();
            ViewBag.dd = adminkontrols;
            if (mlz == "")
            {
                foreach (var item in ViewBag.dd)
                {
                    if (item.kayittarih.Date.Year.ToString() == ylr.ToString())
                    {
                        int temp = item.kayittarih.Date.Month;
                        for (int i = 1; i < 13; i++)
                        {
                            if (temp == i)
                            {
                                if (item.giren == 1)
                                {
                                    giren[i] += item.girilen;
                                    break;
                                }
                                else if (item.giren == 0)
                                {
                                    kalan[i] += item.harcanan;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var item in ViewBag.dd)
                {
                    if (item.kayittarih.Date.Year.ToString() == ylr.ToString())
                    {
                        int temp = item.kayittarih.Date.Month;
                        for (int i = 1; i < 13; i++)
                        {
                            if (temp == i)
                            {
                                if (item.malzemelist.malzeme == mlz)
                                {
                                    if (item.giren == 1)
                                    {
                                        giren[i] += (double)item.girilen;
                                        break;
                                    }
                                    else if (item.giren == 0)
                                    {
                                        kalan[i] += (double)item.harcanan;
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            ViewBag.DataPoints1 = JsonConvert.SerializeObject(giren);
            ViewBag.DataPoints2 = JsonConvert.SerializeObject(kalan);
            ViewBag.ylr = new SelectList(new SelectList(
                                   Enumerable.Range(2020, 50)
                                       .Select(r => new
                                       {
                                           Text = new DateTime(r, 1, 1).ToString("yyyy"),
                                           Value = r.ToString()
                                       }),
                                   "Value", "Text", "").ToList(), "Value", "Text", ylr);
            ViewBag.mlz = new SelectList(db.malzemelist.OrderBy(x => x.malzeme), "malzeme", "malzeme", mlz);
            return View();
        }
        public ActionResult Kisilistele()
        {
            var liste = db.login.OrderByDescending(x => x.id).ToList();
            return View(liste);
        }
        public ActionResult Detail(int id)
        {
            if (id == 1)
            {
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                return View(db.login.Where(x => x.id == id).FirstOrDefault());
            }
        }
        public ActionResult Edit(int id)
        {
            if (id == 1)
            {
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                string a = db.login.Where(x => x.id == id).Select(x => x.status).SingleOrDefault().ToString();
                ViewBag.yetki = new SelectList(db.yetki.OrderBy(x => x.stat), "id", "stat", a);
                return View(db.login.Where(x => x.id == id).FirstOrDefault());
            }
        }
        [HttpPost]
        public ActionResult Edit(login login, int yetki)
        {
            try
            {
                login.status = yetki;
                db.Entry(login).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch
            {
                return RedirectToAction("Kisilistele", "AdminControl");
            }
            return RedirectToAction("Kisilistele", "AdminControl");
        }
        public ActionResult Edit1(int id)
        {
            if (id == 1)
            {
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                return View(db.login.Where(x => x.id == id).FirstOrDefault());
            }
        }
        [HttpPost]
        public ActionResult Edit1(login login)
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
                return RedirectToAction("Kisilistele", "AdminControl");
            }
            return RedirectToAction("Kisilistele", "AdminControl");
        }
        public ActionResult Delete(int id)
        {
            if (id == 1)
            {
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                return View(db.login.Where(x => x.id == id).FirstOrDefault());
            }
        }
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                login login = db.login.Where(x => x.id == id).FirstOrDefault();
                db.login.Remove(login);
                db.SaveChanges();
            }
            catch
            {
                return RedirectToAction("Kisilistele", "AdminControl");
            }
            return RedirectToAction("Kisilistele", "AdminControl");
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
        public ActionResult Tedarikcilistesi()
        {
            var liste = db.tedarikci.OrderByDescending(x => x.id).ToList();
            return View(liste);
        }
        public ActionResult Tedarikciekle()
        {
            tedarikci user = new tedarikci();
            return View(user);
        }
        [HttpPost]
        public ActionResult Tedarikciekle(tedarikci tedarikci)
        {
            db.tedarikci.Add(tedarikci);
            db.SaveChanges();
            return RedirectToAction("Tedarikcilistesi");
        }
        public ActionResult Tedarikdetail(int id)
        {
            return View(db.tedarikci.Where(x => x.id == id).FirstOrDefault());
        }
        public ActionResult Tedarikedit(int id)
        {
            return View(db.tedarikci.Where(x => x.id == id).FirstOrDefault());
        }
        [HttpPost]
        public ActionResult Tedarikedit(tedarikci tedarikci)
        {
            try
            {
                db.Entry(tedarikci).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch
            {
                return RedirectToAction("Tedarikcilistesi");
            }
            return RedirectToAction("Tedarikcilistesi");
        }
        public ActionResult Tedarikdelete(int id)
        {
            return View(db.tedarikci.Where(x => x.id == id).FirstOrDefault());
        }
        [HttpPost]
        public ActionResult Tedarikdelete(int id, FormCollection collection)
        {
            try
            {
                tedarikci tedarikci = db.tedarikci.Where(x => x.id == id).FirstOrDefault();
                db.tedarikci.Remove(tedarikci);
                db.SaveChanges();
            }
            catch
            {
                return RedirectToAction("Tedarikcilistesi");
            }
            return RedirectToAction("Tedarikcilistesi");
        }
        public ActionResult Malzemeekle()
        {
            malzemelist mal = new malzemelist();
            return View(mal);
        }
        [HttpPost]
        public ActionResult Malzemeekle(malzemelist malzemelist)
        {
            if (db.malzemelist.Any(x => x.malzeme == malzemelist.malzeme))
            {
                ViewBag.malzemevar = "Malzeme bulunmakta";
                return View("Malzemeekle", malzemelist);
            }
            else
            {
                db.malzemelist.Add(malzemelist);
                db.SaveChanges();
            }
            return RedirectToAction("Malzemelistesi");
        }
        public ActionResult Malzemelistesi()
        {
            var liste = db.malzemelist.OrderBy(x => x.malzeme).ToList();
            return View(liste);
        }
        public ActionResult Malzemesil(int id)
        {
            try
            {
                malzemelist malzeme = db.malzemelist.Where(x => x.id == id).FirstOrDefault();
                db.malzemelist.Remove(malzeme);
                db.SaveChanges();
            }
            catch
            {
                return RedirectToAction("Malzemelistesi");
            }
            return RedirectToAction("Malzemelistesi");
        }
        public ActionResult Turekle()
        {
            tur tur = new tur();
            return View(tur);
        }
        [HttpPost]
        public ActionResult Turekle(tur tur)
        {
            if (db.tur.Any(x => x.turu == tur.turu))
            {
                ViewBag.turvar = "Tür bulunmakta";
                return View("Turekle", tur);
            }
            else
            {
                db.tur.Add(tur);
                db.SaveChanges();
            }
            return RedirectToAction("Turlistesi");
        }
        public ActionResult Turlistesi()
        {
            var liste = db.tur.OrderBy(x => x.turu).ToList();
            return View(liste);
        }
        public ActionResult Tursil(int id)
        {
            try
            {
                tur tur = db.tur.Where(x => x.id == id).FirstOrDefault();
                db.tur.Remove(tur);
                db.SaveChanges();
            }
            catch
            {
                return RedirectToAction("Turlistesi");
            }
            return RedirectToAction("Turlistesi");
        }
        public ActionResult Birimekle()
        {
            birim birim = new birim();
            return View(birim);
        }
        [HttpPost]
        public ActionResult Birimekle(birim birim)
        {
            if (db.birim.Any(x => x.birim1 == birim.birim1))
            {
                ViewBag.birimvar = "Birim bulunmakta";
                return View("Birimekle", birim);
            }
            else
            {
                db.birim.Add(birim);
                db.SaveChanges();
            }
            return RedirectToAction("Birimlistesi");
        }
        public ActionResult Birimlistesi()
        {
            var liste = db.birim.OrderBy(x => x.birim1).ToList();
            return View(liste);
        }
        public ActionResult Birimsil(int id)
        {
            try
            {
                birim birim = db.birim.Where(x => x.id == id).FirstOrDefault();
                db.birim.Remove(birim);
                db.SaveChanges();
            }
            catch
            {
                return RedirectToAction("Birimlistesi");
            }
            return RedirectToAction("Birimlistesi");
        }

    }
}