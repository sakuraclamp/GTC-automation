using ClosedXML.Excel;
using Microsoft.CodeAnalysis;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using test.Models;

namespace test.Controllers
{
    public class GuvenlikController : Controller
    {
        gtc_stokEntities2 db = new gtc_stokEntities2();
        // GET: Guvenlik
        public ActionResult Index()
        {
            int day = DateTime.Now.Day;
            int count1 = 0;
            int count2 = 0;
            //günlük giriş yapan personelin sayımını yapar
            ViewBag.personel = db.personel.Where(x => x.isdisable == 0).ToList();
            foreach (var item in ViewBag.personel)
            {
                if (item.giris.Date.Day == day)
                {
                    count1++;
                }
            }
            //günlük giriş yapan ziyaretçilerin sayımını yapar
            var misafir = db.misafir.Where(x => x.isdisable == 0).ToList();
            foreach (var item in misafir)
            {
                if (item.girissaat.Value.Day == day)
                {
                    count2++;
                }
            }
            ViewBag.count1 = count1;
            ViewBag.count2 = count2;
            return View();
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
        public ActionResult Personel()
        {
            //dropdown'a personelleri isim-soyisim şeklinde çekerek listeler
            ViewBag.kisi_id = db.personelgenel.Where(x => x.isdisable == 0).OrderBy(x => x.ad).Select(s => new SelectListItem
            {
                Value = s.kart_no.ToString(),
                Text = s.ad + " " + s.soyad.ToString()
            });
            personel user = new personel();
            user.giris = DateTime.Now;
            return View(user);
        }
        [HttpPost]
        public ActionResult Personel(personel personel)
        {
            giriscikis user = new giriscikis();
            user.value = 2;
            user.tarih = Convert.ToDateTime(personel.giris);
            user.zaman = Convert.ToDateTime(personel.giris).TimeOfDay;
            user.kisi_id = personel.kisi_id;
            db.giriscikis.Add(user);
            db.SaveChanges();
            ViewBag.kisi_id = db.personelgenel.Where(x => x.isdisable == 0).OrderBy(x => x.ad).Select(s => new SelectListItem
            {
                Value = s.kart_no.ToString(),
                Text = s.ad + " " + s.soyad.ToString()
            });
            personel.isdisable = 0;
            personel.username = (string)Session["username"];
            personel.giris_id = user.id;
            db.personel.Add(personel);
            db.SaveChanges();
            guvenlikkontrol guvenlik = new guvenlikkontrol();
            guvenlik.misafir = null;
            guvenlik.personel_id = personel.id;
            guvenlik.kul_id = (int)Session["UserId"];
            guvenlik.durum = 0;
            guvenlik.kayittarih = DateTime.Now;
            db.guvenlikkontrol.Add(guvenlik);
            db.SaveChanges();
            return RedirectToAction("Personellistesi");
        }
        public ActionResult Personellistesi()
        {
            return View(db.personel.OrderBy(x => x.cikis).ToList());
        }
        [HttpPost]
        public ActionResult Personellistesi(DateTime date)
        {
            DateTime date1 = date.AddDays(1);
            var user = db.personel.OrderBy(x => x.cikis).Where(x => x.giris > date && x.giris < date1).ToList();
            ViewBag.now7 = date.ToString("yyyy-MM-dd");
            return View(user);
        }
        public ActionResult Ziyaretci()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Ziyaretci(misafir misafir)
        {
            misafir.username = (string)Session["username"];
            misafir.isdisable = 0;
            db.misafir.Add(misafir);
            db.SaveChanges();
            guvenlikkontrol guvenlik = new guvenlikkontrol();
            guvenlik.personel_id = null;
            guvenlik.misafir_id = misafir.id;
            guvenlik.kul_id = (int)Session["UserId"];
            guvenlik.durum = 0;
            guvenlik.kayittarih = DateTime.Now;
            db.guvenlikkontrol.Add(guvenlik);
            db.SaveChanges();
            return RedirectToAction("Ziyaretcilistesi");
        }
        public ActionResult Ziyaretcilistesi()
        {
            return View(db.misafir.OrderBy(x => x.cikissaat).ToList());
        }
        [HttpPost]
        public ActionResult Ziyaretcilistesi(DateTime date)
        {
            DateTime date1 = date.AddDays(1);
            var user = db.misafir.OrderBy(x => x.cikissaat).Where(x => x.girissaat > date && x.girissaat < date1).ToList();
            ViewBag.now6 = date.ToString("yyyy-MM-dd");
            return View(user);
        }
        public ActionResult Personelcikis(int id)
        {
            return View(db.personel.Where(x => x.id == id).FirstOrDefault());
        }
        [HttpPost]
        public ActionResult Personelcikis(personel personel)
        {
            var personel1 = db.personel.Where(x => x.id == personel.id).FirstOrDefault();
            personel1.cikis = personel.cikis;
            personel1.username = (string)Session["username"];
            giriscikis user = new giriscikis();
            user.value = 3;
            user.tarih = personel.cikis;
            user.zaman = Convert.ToDateTime(personel.cikis).TimeOfDay;
            user.kisi_id = personel1.kisi_id;
            db.giriscikis.Add(user);
            db.SaveChanges();
            personel1.cikis_id = user.id;
            db.Entry(personel1).State = EntityState.Modified;
            db.SaveChanges();
            guvenlikkontrol guvenlik = new guvenlikkontrol();
            guvenlik.misafir = null;
            guvenlik.personel_id = personel.id;
            guvenlik.kul_id = (int)Session["UserId"];
            guvenlik.durum = 1;
            guvenlik.kayittarih = DateTime.Now;
            db.guvenlikkontrol.Add(guvenlik);
            db.SaveChanges();
            return RedirectToAction("Personellistesi");
        }
        public ActionResult Ziyaretcicikis(int id)
        {
            return View(db.misafir.Where(x => x.id == id).FirstOrDefault());
        }
        [HttpPost]
        public ActionResult Ziyaretcicikis(misafir misafir)
        {
            var misafir1 = db.misafir.Where(x => x.id == misafir.id).FirstOrDefault();
            misafir1.cikissaat = misafir.cikissaat;
            misafir1.username = (string)Session["username"];
            db.Entry(misafir1).State = EntityState.Modified;
            db.SaveChanges();
            guvenlikkontrol guvenlik = new guvenlikkontrol();
            guvenlik.personel_id = null;
            guvenlik.misafir_id = misafir.id;
            guvenlik.kul_id = (int)Session["UserId"];
            guvenlik.durum = 1;
            guvenlik.kayittarih = DateTime.Now;
            db.guvenlikkontrol.Add(guvenlik);
            db.SaveChanges();
            return RedirectToAction("Ziyaretcilistesi");
        }
        public ActionResult Getir(int id)
        {
            var user = db.misafir.Where(x => x.id == id).FirstOrDefault();
            user.girissaat = DateTime.Now;
            return View(user);
        }
        public ActionResult Personelsil(int id)
        {
            var user = db.personel.Where(x => x.id == id).FirstOrDefault();
            user.isdisable = 1;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            //personel silme işleminde sadece disable yapıldığı için bu personele ait girilen giriş-çıkış verilerinin silinmesi sağlanır.
            if (user.giris_id != null)
            {
                var islemg = db.giriscikis.Where(x => x.id == user.giris_id).FirstOrDefault();
                db.giriscikis.Remove(islemg);
                db.SaveChanges();
            }
            if (user.cikis_id != null)
            {
                var islemc = db.giriscikis.Where(x => x.id == user.cikis_id).FirstOrDefault();
                db.giriscikis.Remove(islemc);
                db.SaveChanges();
            }
            guvenlikkontrol guvenlik = new guvenlikkontrol();
            guvenlik.personel_id = user.id;
            guvenlik.misafir_id = null;
            guvenlik.kul_id = (int)Session["UserId"];
            guvenlik.durum = 2;
            guvenlik.kayittarih = DateTime.Now;
            db.guvenlikkontrol.Add(guvenlik);
            db.SaveChanges();
            return RedirectToAction("Personellistesi");
        }
        public ActionResult Ziyaretcisil(int id)
        {
            var user = db.misafir.Where(x => x.id == id).FirstOrDefault();
            user.isdisable = 1;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            guvenlikkontrol guvenlik = new guvenlikkontrol();
            guvenlik.personel_id = null;
            guvenlik.misafir_id = user.id;
            guvenlik.kul_id = (int)Session["UserId"];
            guvenlik.durum = 2;
            guvenlik.kayittarih = DateTime.Now;
            db.guvenlikkontrol.Add(guvenlik);
            db.SaveChanges();
            return RedirectToAction("Ziyaretcilistesi");
        }
        public ActionResult Sicaklik()
        {
            
            ViewBag.kisi_id = db.personelgenel.Where(x => x.isdisable == 0).OrderBy(x => x.ad).Select(s => new SelectListItem
            {
                Value = s.kart_no.ToString(),
                Text = s.ad + " " + s.soyad.ToString()
            });
            return View();
        }
        [HttpPost]
        public ActionResult Sicaklik(personelsicaklik personel)
        {
            db.personelsicaklik.Add(personel);
            db.SaveChanges();
            ViewBag.kisi_id = db.personelgenel.Where(x => x.isdisable == 0).OrderBy(x => x.ad).Select(s => new SelectListItem
            {
                Value = s.kart_no.ToString(),
                Text = s.ad + " " + s.soyad.ToString()
            });
            ViewBag.basarili = "Kaydedildi";
            return View();
        }
        public ActionResult Sicakliklistele()
        {
            return View(db.personelsicaklik.OrderByDescending(x => x.id).ToList());
        }
        public ActionResult Sicakliksil(int id)
        {
            var user = db.personelsicaklik.Where(x => x.id == id).FirstOrDefault();
            db.personelsicaklik.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Sicakliklistele");
        }
        public ActionResult Aylik()
        {
            var yil = DateTime.Now.Year;
            var ay = DateTime.Now.Month;
            var myDate = new DateTime(yil, ay, 1);
            ViewBag.mydate = myDate;
            ViewBag.yil = new SelectList(new SelectList(
                      Enumerable.Range(2017, 50)
                          .Select(r => new
                          {
                              Text = new DateTime(r, 1, 1).ToString("yyyy"),
                              Value = r.ToString()
                          }),
                      "Value", "Text", string.Empty).ToList(), "Value", "Text", yil.ToString());
            ViewBag.ay = new SelectList(new SelectList(
          Enumerable.Range(1, 12)
              .Select(r => new
              {
                  Text = new DateTime(1, r, 1).ToString("MM"),
                  Value = r.ToString()
              }),
          "Value", "Text", string.Empty).ToList(), "Value", "Text", ay.ToString());
            ViewBag.day = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            ViewBag.kisiler = db.personelgenel.Where(x => x.isdisable == 0).ToList();
            int j = db.personelgenel.Where(x => x.isdisable == 0).Count();
            string[,] giris = new string[j + 1, ViewBag.day + 1];
            int count = 0;
            ViewBag.dd = db.personelsicaklik.OrderBy(x => x.kisi_id).Where(x => x.tarih >= myDate).ToList();
            foreach (var item in ViewBag.kisiler)
            {
                count++;
                foreach (var item1 in ViewBag.dd)
                {
                    if (item1.tarih.Date.Month == ay)
                    {
                        if (item.kart_no == item1.kisi_id)
                        {
                            for (int i = 1; i <= ViewBag.day; i++)
                            {
                                if (item1.tarih.Date.Day == i)
                                {
                                    giris[count, i] = item1.sicaklik.ToString();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            ViewBag.giris = giris;
            return View();
        }
        [HttpPost]
        public ActionResult Aylik(int yil, int ay)
        {
            var myDate = new DateTime(yil, ay, 1);
            ViewBag.mydate = myDate;
            ViewBag.yil = new SelectList(new SelectList(
                      Enumerable.Range(2017, 50)
                          .Select(r => new
                          {
                              Text = new DateTime(r, 1, 1).ToString("yyyy"),
                              Value = r.ToString()
                          }),
                      "Value", "Text", string.Empty).ToList(), "Value", "Text", yil.ToString());
            ViewBag.ay = new SelectList(new SelectList(
          Enumerable.Range(1, 12)
              .Select(r => new
              {
                  Text = new DateTime(1, r, 1).ToString("MM"),
                  Value = r.ToString()
              }),
          "Value", "Text", string.Empty).ToList(), "Value", "Text", ay.ToString());
            ViewBag.day = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            ViewBag.kisiler = db.personelgenel.Where(x => x.isdisable == 0).ToList();
            int j = db.personelgenel.Where(x => x.isdisable == 0).Count();
            string[,] giris = new string[j + 1, ViewBag.day + 1];
            int count = 0;
            ViewBag.dd = db.personelsicaklik.OrderBy(x => x.kisi_id).Where(x => x.tarih >= myDate).ToList();
            foreach (var item in ViewBag.kisiler)
            {
                count++;
                foreach (var item1 in ViewBag.dd)
                {
                    if (item1.tarih.Date.Month == ay)
                    {
                        if (item.kart_no == item1.kisi_id)
                        {
                            for (int i = 1; i <= ViewBag.day; i++)
                            {
                                if (item1.tarih.Date.Day == i)
                                {
                                    giris[count, i] = item1.sicaklik.ToString();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            ViewBag.giris = giris;
            return View();
        }
        public ActionResult AylikExcel(int yil, int ay)
        {
            var myDate = new DateTime(yil, ay, 1);
            ViewBag.mydate = myDate;
            ViewBag.kisiler = db.personelgenel.Where(x => x.isdisable == 0).ToList();
            int j = db.personelgenel.Where(x => x.isdisable == 0).Count();
            ViewBag.day = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            string[,] giris = new string[j + 1, ViewBag.day + 1];
            int count = 0;
            ViewBag.dd = db.personelsicaklik.OrderBy(x => x.kisi_id).Where(x => x.tarih >= myDate).ToList();
            foreach (var item in ViewBag.kisiler)
            {
                count++;
                foreach (var item1 in ViewBag.dd)
                {
                    if (item1.tarih.Date.Month == ay)
                    {
                        if (item.kart_no == item1.kisi_id)
                        {
                            for (int i = 1; i <= ViewBag.day; i++)
                            {
                                if (item1.tarih.Date.Day == i)
                                {
                                    giris[count, i] = item1.sicaklik.ToString();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            ViewBag.giris = giris;
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sıcaklık");
                var rows = 0;
                var columnA = worksheet.Column("A");
                columnA.Width = 25;
                worksheet.Columns("B:AF").Width = 5;
                foreach (var item in ViewBag.kisiler)
                {
                    rows++;
                    worksheet.Cell(rows + 1, 1).Value = item.ad + " " + item.soyad;
                    for (int i = 1; i <= ViewBag.day; i++)
                    {

                        worksheet.Cell(1, i + 1).Value = i;
                        worksheet.Cell(rows + 1, i + 1).Value = giris[rows, i];
                    }
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Sıcaklık.xlsx");
                }
            }
        }
    }
}