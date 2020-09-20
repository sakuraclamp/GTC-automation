using ClosedXML.Excel;
using System;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using test.Models;


namespace test.Controllers
{
    public class StokController : Controller
    {
        gtc_stokEntities2 db = new gtc_stokEntities2();

        // GET: Stok
        public ActionResult Stokekle(int id = 0)
        {
            ViewBag.malzeme_id = new SelectList(db.malzemelist.OrderBy(x => x.malzeme), "id", "malzeme");
            ViewBag.tedarik_id = new SelectList(db.tedarikci.OrderBy(x => x.firma), "id", "firma");
            ViewBag.birim_id = new SelectList(db.birim.OrderBy(x => x.birim1), "id", "birim1");
            ViewBag.tur_id = new SelectList(db.tur.OrderBy(x => x.turu), "id", "turu");
            stok zaman = new stok();
            zaman.tarih = DateTime.Now;
            return View(zaman);
        }
        [HttpPost]
        public ActionResult Stokekle(stok stok, adminkontrol adminkontrol)
        {
            //Kullanıcının username'ne ait id'yi kul_id'de tutar //
            stok.kul_id = (int)Session["UserId"];
            stok.isdisable = 0;
            stok.kalanmiktar = stok.miktar;
            db.stok.Add(stok);
            db.SaveChanges();
            //log kayıtlarını almak için
            adminkontrol.birim = stok.birim_id;
            adminkontrol.malzeme = stok.malzeme_id;
            adminkontrol.tur = stok.tur_id;
            adminkontrol.tedarik = stok.tedarik_id;
            adminkontrol.kul_id = (int)Session["UserId"];
            adminkontrol.kayittarih = DateTime.Now;
            adminkontrol.stok_id = stok.id;
            adminkontrol.miktar = stok.kalanmiktar;
            adminkontrol.girilen = stok.kalanmiktar;
            adminkontrol.harcanan = 0;
            //adminkontrol.giren=1 değer için; ürün girdi yazılır
            adminkontrol.giren = 1;
            db.adminkontrol.Add(adminkontrol);
            db.SaveChanges();
            ModelState.Clear();
            return RedirectToAction("Stoklistele");
        }
        public ActionResult Stoklistele()
        {
            ViewBag.end = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            ViewBag.start = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
            var liste = db.stok.OrderByDescending(x => x.id).ToList();
            return View(liste);
        }
        public ActionResult StokExcel(DateTime start, DateTime end)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Users");
                var rows = 1;
                var columnB = worksheet.Column("B");
                columnB.Width = 15;
                var rows1 = worksheet.Row(1);
                rows1.Style.Font.Bold = true;
                var columnC = worksheet.Column("C");
                columnC.Width = 14;
                var columnD = worksheet.Column("D");
                columnD.Width = 25;
                var columnE = worksheet.Column("E");
                columnE.Width = 15;
                var columnF = worksheet.Column("F");
                columnF.Width = 8;
                var columnH = worksheet.Column("H");
                columnH.Width = 15;
                var columnI = worksheet.Column("I");
                columnI.Width = 13;
                var columnJ = worksheet.Column("J");
                columnJ.Width = 13;
                worksheet.Cell(rows, 1).Value = "Id";
                worksheet.Cell(rows, 2).Value = "Stok Giriş Tarihi";
                worksheet.Cell(rows, 3).Value = "Kullanıcı";
                worksheet.Cell(rows, 4).Value = "Tedarikçi Firma";
                worksheet.Cell(rows, 5).Value = "Türü";
                worksheet.Cell(rows, 6).Value = "Miktar";
                worksheet.Cell(rows, 7).Value = "Birim";
                worksheet.Cell(rows, 8).Value = "Malzeme";
                var userDetail = db.stok.Where(x => x.tarih >= start && x.tarih < end).ToList();
                var liste = db.stok.ToList();
                ViewBag.list = userDetail;
                foreach (var user in ViewBag.list)
                {
                    rows++;
                    worksheet.Cell(rows, 1).Value = user.id;
                    worksheet.Cell(rows, 2).Value = user.tarih;
                    worksheet.Cell(rows, 3).Value = user.login.name;
                    worksheet.Cell(rows, 4).Value = user.tedarikci.firma;
                    worksheet.Cell(rows, 5).Value = user.tur.turu;
                    worksheet.Cell(rows, 6).Value = user.miktar;
                    worksheet.Cell(rows, 7).Value = user.birim.birim1;
                    worksheet.Cell(rows, 8).Value = user.malzemelist.malzeme;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Stok.xlsx");
                }
            }
        }
        public ActionResult Adminstoklistele()
        {
            ViewBag.d1 = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            ViewBag.d2 = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
            var liste = db.adminkontrol.OrderByDescending(x => x.id).ToList();

            return View(liste);
        }

        //Log kayıtlarını Excel'e aktarma
        [HttpPost]
        public ActionResult LogExcel(DateTime start, DateTime end)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Users");
                var rows = 1;
                var columnB = worksheet.Column("B");
                columnB.Width = 15;
                var rows1 = worksheet.Row(1);
                rows1.Style.Font.Bold = true;
                var columnC = worksheet.Column("C");
                columnC.Width = 14;
                var columnD = worksheet.Column("D");
                columnD.Width = 15;
                var columnE = worksheet.Column("E");
                columnE.Width = 25;
                var columnF = worksheet.Column("F");
                columnF.Width = 13;
                var columnH = worksheet.Column("H");
                columnH.Width = 13;
                var columnI = worksheet.Column("I");
                columnI.Width = 13;
                var columnJ = worksheet.Column("J");
                columnJ.Width = 13;
                worksheet.Cell(rows, 1).Value = "Id";
                worksheet.Cell(rows, 2).Value = "Log Kayıt Tarihi";
                worksheet.Cell(rows, 3).Value = "Stok Giriş Tarihi";
                worksheet.Cell(rows, 4).Value = "Kullanıcı";
                worksheet.Cell(rows, 5).Value = "Tedarikçi Firma";
                worksheet.Cell(rows, 6).Value = "Türü";
                worksheet.Cell(rows, 7).Value = "Miktar";
                worksheet.Cell(rows, 8).Value = "Birim";
                worksheet.Cell(rows, 9).Value = "Malzeme";
                worksheet.Cell(rows, 10).Value = "Durum";
                var userDetail = db.adminkontrol.Where(x => x.kayittarih >= start && x.kayittarih <= end).ToList();
                var liste = db.adminkontrol.ToList();
                ViewBag.list = userDetail;
                foreach (var user in ViewBag.list)
                {
                    rows++;
                    worksheet.Cell(rows, 1).Value = user.id;
                    worksheet.Cell(rows, 2).Value = user.kayittarih;
                    worksheet.Cell(rows, 3).Value = user.stok.tarih;
                    worksheet.Cell(rows, 4).Value = user.login.name;
                    worksheet.Cell(rows, 5).Value = user.tedarikci.firma;
                    worksheet.Cell(rows, 6).Value = user.tur1.turu;
                    worksheet.Cell(rows, 7).Value = user.miktar;
                    worksheet.Cell(rows, 8).Value = user.birim1.birim1;
                    worksheet.Cell(rows, 9).Value = user.malzemelist.malzeme;
                    if (user.giren == 1)
                    {
                        worksheet.Cell(rows, 10).Value = "Ürün Girdi";
                    }
                    else if (user.giren == 2)
                    {
                        for (int i = 1; i < 11; i++)
                        {
                            worksheet.Cell(rows, i).Style.Font.SetFontColor(XLColor.Blue);
                        }
                        worksheet.Cell(rows, 10).Value = "Güncellendi";
                    }
                    else if (user.giren == 3)
                    {
                        for (int i = 1; i < 11; i++)
                        {
                            worksheet.Cell(rows, i).Style.Font.SetFontColor(XLColor.Red);
                        }
                        worksheet.Cell(rows, 10).Value = "Silindi";
                    }
                    else
                    {
                        worksheet.Cell(rows, 10).Value = "Ürün Kaldı";
                    }
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "StokLog.xlsx");
                }
            }
        }
        public ActionResult Stokdetail(int id)
        {
            return View(db.adminkontrol.Where(x => x.id == id).FirstOrDefault());
        }

        //public ActionResult Stokedit(int? id)
        //{
        //    string a = db.stok.Where(x => x.id == id).Select(x => x.malzeme_id).SingleOrDefault().ToString();
        //    string b = db.stok.Where(x => x.id == id).Select(x => x.tedarik_id).SingleOrDefault().ToString();
        //    string c = db.stok.Where(x => x.id == id).Select(x => x.birim_id).SingleOrDefault().ToString();
        //    string d = db.stok.Where(x => x.id == id).Select(x => x.tur_id).SingleOrDefault().ToString();
        //    ViewBag.malzeme_id = new SelectList(db.malzemelist.OrderBy(x => x.malzeme), "id", "malzeme", a);
        //    ViewBag.tedarik_id = new SelectList(db.tedarikci.OrderBy(x => x.firma), "id", "firma", b);
        //    ViewBag.birim_id = new SelectList(db.birim.OrderBy(x => x.birim1), "id", "birim1", c);
        //    ViewBag.tur_id = new SelectList(db.tur.OrderBy(x => x.turu), "id", "turu", d);
        //    return View(db.stok.Where(x => x.id == id).FirstOrDefault());
        //}
        //[HttpPost]
        //public ActionResult Stokedit(adminkontrol adminkontrol, stok stok)
        //{

        //    //log kayıtlarının yer aldığı admin kontrol database için editleme işlemi yapılmayacak
        //    adminkontrol.kul_id = (int)Session["UserId"];
        //    adminkontrol.birim = stok.birim_id;
        //    adminkontrol.harcanan = 0;
        //    adminkontrol.malzeme = stok.malzeme_id;
        //    adminkontrol.tur = stok.tur_id;
        //    adminkontrol.tedarik = stok.tedarik_id;
        //    adminkontrol.giren = 2;
        //    adminkontrol.stok_id = stok.id;
        //    adminkontrol.kayittarih = DateTime.Now;
        //    adminkontrol.kul_id = (int)Session["UserId"];
        //    adminkontrol.miktar = stok.miktar;
        //    stok.kalanmiktar = stok.miktar;
        //    db.adminkontrol.Add(adminkontrol);
        //    db.SaveChanges();
        //    try
        //    {
        //        stok.kul_id = (int)Session["UserId"];
        //        stok.isdisable = 0;
        //        db.Entry(stok).State = EntityState.Modified;
        //        db.SaveChanges();

        //    }
        //    catch
        //    {
        //        return RedirectToAction("Index", "Admin");
        //    }
        //    return RedirectToAction("Stoklistele");
        //}
        public ActionResult Stokdelete(int id)
        {
            return View(db.stok.Where(x => x.id == id).FirstOrDefault());
        }
        [HttpPost]
        public ActionResult Stokdelete(int id, FormCollection collection)
        {
            stok stok = db.stok.Where(x => x.id == id).FirstOrDefault();
            adminkontrol adminkontrol = db.adminkontrol.Where(x => x.stok_id == id).FirstOrDefault();
            var adminkontrols = db.adminkontrol.Where(x => x.stok_id == id).ToList();
            ViewBag.dd = adminkontrols;
            foreach (var item in ViewBag.dd)
            {
                item.girilen = 0;
                item.harcanan = 0;
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
            }
            adminkontrol.kayittarih = DateTime.Now;
            adminkontrol.miktar = 0;
            adminkontrol.harcanan = 0;
            adminkontrol.stok.isdisable = 1;
            adminkontrol.stok.miktar = 0;
            adminkontrol.kul_id = (int)Session["UserId"];
            adminkontrol.stok.kalanmiktar = 0;
            adminkontrol.giren = 3;
            adminkontrol.girilen = 0;
            db.adminkontrol.Add(adminkontrol);
            db.SaveChanges();
            return RedirectToAction("Stoklistele");
        }
        public ActionResult disable(int id)
        {
            return View(db.stok.Where(x => x.id == id).FirstOrDefault());
        }
        [HttpPost]
        public ActionResult disable(stok stok, adminkontrol adminkontrol)
        {
            //adminkontrol tarafında ayrıca harcanan miktarın tutulması için kullanılır
            adminkontrol.harcanan = stok.kalanmiktar;
            var temp = stok.miktar;
            stok.miktar -= stok.kalanmiktar;
            if (stok.miktar < 0)
            {
                stok.miktar = temp;
                ViewBag.fazlalik = "Miktardan küçük değer girin";
                return View(db.stok.Where(x => x.id == stok.id).FirstOrDefault());
            }
            else
            {
                stok.kalanmiktar = stok.miktar;
            }
            try
            {
                if (stok.kalanmiktar == 0)
                {
                    stok.isdisable = 1;
                }
                else
                {
                    stok.isdisable = 0;
                }
                stok.kul_id = (int)Session["UserId"];
                db.Entry(stok).State = EntityState.Modified;
                db.SaveChanges();
                // güncelleme yapılırken bile yeni kayıt alma ile bu güncel bilgiler eklenir
                // giren adminkontrol.değeri= 0 için; kalan değer yazdırılır
                adminkontrol.kul_id = (int)Session["UserId"];  //güncelleme yapılırken kullanıcı bilgilerini tutmak için id çekme işlemi
                adminkontrol.birim = stok.birim_id;
                adminkontrol.malzeme = stok.malzeme_id;
                adminkontrol.tur = stok.tur_id;
                adminkontrol.tedarik = stok.tedarik_id;
                adminkontrol.kayittarih = DateTime.Now;
                adminkontrol.stok_id = stok.id;
                adminkontrol.miktar = stok.kalanmiktar;
                adminkontrol.giren = 0;
                adminkontrol.girilen = 0;
                db.adminkontrol.Add(adminkontrol);
                db.SaveChanges();

            }
            catch
            {
                return RedirectToAction("Index", "Admin");
            }
            return RedirectToAction("Stoklistele", "Stok");
        }
        public ActionResult veriler(int ylr, string mlz)
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
            var result = new { giren, kalan };
            return Json(result, JsonRequestBehavior.AllowGet);
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
                return RedirectToAction("Index", "Admin");
            }
            return RedirectToAction("Index", "Admin");
        }
    }
}