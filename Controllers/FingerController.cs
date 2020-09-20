using ClosedXML.Excel;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using test.Models;

namespace test.Controllers
{
    public class FingerController : Controller
    {
        // GET: Finger
        gtc_stokEntities2 db = new gtc_stokEntities2();
        public ActionResult Index()
        {
            ViewBag.kisiler = db.personelgenel.Where(x => x.isdisable == 0).Count();
            return View();
        }
        public ActionResult Personelekle()
        {
            //dropdown için gerekli verileri göndermesi sağlanır
            ViewBag.grup_id = new SelectList(db.personelgrup.OrderBy(x => x.grupadi), "id", "grupadi");
            ViewBag.gorev_id = new SelectList(db.personelgorev.OrderBy(x => x.gorevadi), "id", "gorevadi");
            ViewBag.bolum_id = new SelectList(db.personelbolum.OrderBy(x => x.blmadi), "id", "blmadi");
            ViewBag.egitim_id = new SelectList(db.personelegitim.OrderBy(x => x.id), "id", "egitimadi");
            ViewBag.medenihali_id = new SelectList(db.medenihali.OrderBy(x => x.id), "id", "medeni_hali");
            ViewBag.kangrubu_id = new SelectList(db.kangrup.OrderBy(x => x.id), "id", "kan");
            ViewBag.cinsiyet_id = new SelectList(db.cinsiyet.OrderBy(x => x.cinsiyet_adi), "id", "cinsiyet_adi");
            personelgenel user = new personelgenel();
            var test = db.personelgenel.OrderByDescending(x => x.kart_no).Take(1).ToList();
            ViewBag.dd = test;
            //her personel için gerekli kart no aynı olmaması adına otomatik arttırma sağlanır
            foreach (var item in ViewBag.dd)
            {
                user.kart_no = item.kart_no + 1;
            }
            user.baslamatarihi = DateTime.Now;
            user.resmibaslamatarihi = DateTime.Now;
            return View(user);
        }
        [HttpPost]
        public ActionResult Personelekle(personelgenel personel)
        {
            ViewBag.grup_id = new SelectList(db.personelgrup.OrderBy(x => x.grupadi), "id", "grupadi");
            ViewBag.gorev_id = new SelectList(db.personelgorev.OrderBy(x => x.gorevadi), "id", "gorevadi");
            ViewBag.bolum_id = new SelectList(db.personelbolum.OrderBy(x => x.blmadi), "id", "blmadi");
            ViewBag.egitim_id = new SelectList(db.personelegitim.OrderBy(x => x.id), "id", "egitimadi");
            if (db.personelgenel.Any(x => x.kart_no == personel.kart_no))
            {
                ViewBag.hata = "Kard ID bulunmakta";
                return View(personel);
            }
            else
            {
                personel.isdisable = 0;
                db.personelgenel.Add(personel);
                db.SaveChanges();
                return RedirectToAction("Detail/" + personel.kart_no, "Finger");
            }
        }
        public ActionResult Personellistele()
        {
            return View(db.personelgenel.OrderByDescending(x => x.kart_no).ToList());
        }
        public ActionResult Ekle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Ekle(personelgenel personelgenel)
        {
            if (personelgenel.personelbolum.blmadi != null)
            {
                db.personelbolum.Add(personelgenel.personelbolum);
                db.SaveChanges();
            }
            if (personelgenel.personelgorev.gorevadi != null)
            {
                db.personelgorev.Add(personelgenel.personelgorev);
                db.SaveChanges();
            }
            if (personelgenel.personelgrup.grupadi != null)
            {
                db.personelgrup.Add(personelgenel.personelgrup);
                db.SaveChanges();
            }
            ModelState.Clear();
            return RedirectToAction("Personellistele", "Finger");
        }
        public ActionResult Detail(int id)
        {
            ViewBag.medenihali_id = new SelectList(db.medenihali.OrderBy(x => x.id), "id", "medeni_hali");
            ViewBag.kangrubu_id = new SelectList(db.kangrup.OrderBy(x => x.id), "id", "kan");
            ViewBag.cinsiyet_id = new SelectList(db.cinsiyet.OrderBy(x => x.cinsiyet_adi), "id", "cinsiyet_adi");
            return View(db.personelgenel.Where(x => x.kart_no == id).FirstOrDefault());
        }
        [HttpPost]
        public ActionResult Detail(personelgenel personelgenel)
        {
            try
            {
                db.Entry(personelgenel).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch
            {
                return RedirectToAction("Detail", "Finger");
            }
            return RedirectToAction("Personellistele", "Finger");
        }
        public ActionResult Personeledit(int id)
        {
            ViewBag.grup_id = new SelectList(db.personelgrup.OrderBy(x => x.grupadi), "id", "grupadi");
            ViewBag.gorev_id = new SelectList(db.personelgorev.OrderBy(x => x.gorevadi), "id", "gorevadi");
            ViewBag.bolum_id = new SelectList(db.personelbolum.OrderBy(x => x.blmadi), "id", "blmadi");
            ViewBag.egitim_id = new SelectList(db.personelegitim.OrderBy(x => x.id), "id", "egitimadi");
            ViewBag.medenihali_id = new SelectList(db.medenihali.OrderBy(x => x.id), "id", "medeni_hali");
            ViewBag.kangrubu_id = new SelectList(db.kangrup.OrderBy(x => x.id), "id", "kan");
            ViewBag.cinsiyet_id = new SelectList(db.cinsiyet.OrderBy(x => x.cinsiyet_adi), "id", "cinsiyet_adi");
            return View(db.personelgenel.Where(x => x.kart_no == id).FirstOrDefault());
        }
        [HttpPost]
        public ActionResult Personeledit(personelgenel personel, HttpPostedFileBase file)
        {
            //personele ait profil resminin yüklenmesi
            if (file != null)
            {
                //eklenin öğrenin adını kart no olarak kaydeder
                string pic = System.IO.Path.GetFileName("Profile_" + personel.kart_no + ".jpg");
                string path = System.IO.Path.Combine(
                                       Server.MapPath("~/Content/images"), pic);
                file.SaveAs(path);
                using (MemoryStream ms = new MemoryStream())
                {
                    file.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }
                personel.image = pic.ToString();
            }
            else if (file == null)
            {
                personel.image = db.personelgenel.Where(x => x.kart_no == personel.kart_no).Select(x => x.image).FirstOrDefault();
            }
            db.Entry(personel).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Detail/" + personel.kart_no, "Finger");
        }
        public ActionResult Disable(int id)
        {
            var user = db.personelgenel.Where(x => x.kart_no == id).FirstOrDefault();
            user.cikistarihi = DateTime.Now;
            return View(user);
        }
        [HttpPost]
        public ActionResult Disable(personelgenel personel)
        {
            var user = db.personelgenel.Where(x => x.kart_no == personel.kart_no).FirstOrDefault();
            user.isdisable = 1;
            user.cikistarihi = personel.cikistarihi;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Cikanlar", "Finger");
        }
        public ActionResult Cikanlar()
        {
            return View(db.personelgenel.OrderByDescending(x => x.kart_no).ToList());
        }
        public ActionResult Gerial(int id)
        {
            var user = db.personelgenel.Where(x => x.kart_no == id).FirstOrDefault();
            user.baslamatarihi = DateTime.Now;
            return View(user);
        }
        [HttpPost]
        public ActionResult Gerial(personelgenel personel)
        {
            var user = db.personelgenel.Where(x => x.kart_no == personel.kart_no).FirstOrDefault();
            user.isdisable = 0;
            db.SaveChanges();
            return RedirectToAction("Personellistele", "Finger");
        }
        public ActionResult Delete(int id)
        {
            return View(db.personelgenel.Where(x => x.kart_no == id).SingleOrDefault());
        }
        [HttpPost]
        public ActionResult Delete(int id, personelgenel personel)
        {
            try
            {
                personel = db.personelgenel.Where(x => x.kart_no == id).FirstOrDefault();
                db.personelgenel.Remove(personel);
                db.SaveChanges();
            }
            catch
            {
                return RedirectToAction("Detail", "Finger");
            }
            return RedirectToAction("Personellistele", "Finger");
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
        public ActionResult Aktar()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Aktar(HttpPostedFileBase[] file, giriscikis giris)
        {
            //birden fazla dosya yükleme durumunda gerekli olan işlemlerin yapılması
            foreach (HttpPostedFileBase files in file)
            {
                if (files != null)
                {
                    string path = System.IO.Path.Combine(
                                               Server.MapPath("~/document"), files.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.durum = "Dosya bulunmaktadır. Lütfen farklı bir tane seçiniz.";
                        return View();
                    }
                    else
                    {
                        files.SaveAs(path);
                        DataTable dt = csv.ReadCsv(path);
                        //txt dosyasındaki verilerin gerekli kısımlarının kaydedilmesini sağlar
                        foreach (DataRow row in dt.Rows)
                        {
                            giris.kisi_id = Convert.ToInt32(row[0]);
                            string zaman = string.Join(" ", row[1], row[2]);
                            giris.tarih = Convert.ToDateTime(zaman);
                            giris.zaman = Convert.ToDateTime(row[2]).TimeOfDay;
                            int temp = Convert.ToInt32(row[4]);
                            if (temp % 2 == 0)
                            {
                                giris.value = 0;
                            }
                            else
                            {
                                giris.value = 1;
                            }
                            var userDetail = db.personelgenel.Where(x => x.kart_no == giris.kisi_id).FirstOrDefault();
                            if (userDetail != null)
                            {
                                db.giriscikis.Add(giris);
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
            return RedirectToAction("Giriscikis");
        }
        public ActionResult Giriscikis()
        {
            return View(db.giriscikis.OrderByDescending(x => x.tarih).ToList());
        }
        public ActionResult Degistir(giriscikis giris)
        {
            var userDetail = db.giriscikis.Where(x => x.id == giris.id).FirstOrDefault();
            //giriş-çıkış değelerinin yer değiştirmesi sağlanır
            if (userDetail.value == 0)
            {
                userDetail.value = 1;
            }
            else if (userDetail.value == 1)
            {
                userDetail.value = 0;
            }
            db.Entry(userDetail).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Giriscikis");
        }
        public ActionResult Goster(int id)
        {
            string[] giris = new string[31];
            string[] cikis = new string[31];
            string[] giris1 = new string[31];
            string[] cikis1 = new string[31];
            ViewBag.kisi = db.personelgenel.Where(x => x.kart_no == id).FirstOrDefault();
            var userDetail = db.giriscikis.OrderByDescending(x => x.tarih).Where(x => x.kisi_id == id).ToList();
            ViewBag.dd = userDetail;
            //belirlenmiş tarihteki bir ayda kaç gün olduğunu belirler
            int a = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            ViewBag.a = a;
            ViewBag.end = DateTime.Now;
            ViewBag.d1 = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            ViewBag.d2 = DateTime.Now.AddDays(-(a - 1)).ToString("yyyy-MM-dd");
            ViewBag.DataPoints1 = JsonConvert.SerializeObject(a);
            //her bir güne ait giriş ve çıkış değerlerini bulur
            foreach (var item in ViewBag.dd)
            {
                for (int i = 0; i < a; i++)
                {
                    if (item.tarih.ToString("0:dd/MM/yyyy") == DateTime.Now.AddDays(-i).ToString("0:dd/MM/yyyy"))
                    {
                        if (item.value == 0 || item.value == 2)
                        {
                            if (item.value == 0)
                            {
                                giris[i] = item.zaman.ToString("hh\\:mm");
                            }
                            else
                            {
                                giris1[i] = item.zaman.ToString("hh\\:mm");
                            }
                            break;
                        }
                        else if (item.value == 1 || item.value == 3)
                        {
                            if (item.value == 1)
                            {
                                cikis[i] = item.zaman.ToString("hh\\:mm");
                            }
                            else
                            {
                                cikis1[i] = item.zaman.ToString("hh\\:mm");
                            }
                            break;
                        }
                    }
                }
            }
            for (int i = 0; i < a; i++)
            {
                if (giris[i] == null)
                {
                    giris[i] = "5555";
                }
                if (giris1[i] == null)
                {
                    giris1[i] = "5555";
                }
                if (cikis[i] == null)
                {
                    cikis[i] = "0";
                }
                if (cikis1[i] == null)
                {
                    cikis1[i] = "0";
                }
                if (int.Parse(giris[i].Replace(":", "")) > int.Parse(giris1[i].Replace(":", "")))
                {
                    giris[i] = giris1[i];
                }
                if (int.Parse(cikis[i].Replace(":", "")) < int.Parse(cikis1[i].Replace(":", "")))
                {
                    cikis[i] = cikis1[i];
                }
                if (giris[i] == "5555")
                {
                    giris[i] = null;
                }
                if (giris1[i] == "5555")
                {
                    giris1[i] = null;
                }
                if (cikis[i] == "0")
                {
                    cikis[i] = null;
                }
                if (cikis1[i] == "0")
                {
                    cikis1[i] = null;
                }
            }
            ViewBag.giris = giris;
            ViewBag.cikis = cikis;
            return View();
        }
        [HttpPost]
        public ActionResult Goster(DateTime start, DateTime end, int id)
        {
            TimeSpan diff = end - start;
            int a = diff.Days;
            string[] giris = new string[a];
            string[] cikis = new string[a];
            string[] giris1 = new string[a];
            string[] cikis1 = new string[a];
            ViewBag.kisi = db.personelgenel.Where(x => x.kart_no == id).FirstOrDefault();
            var userDetail = db.giriscikis.OrderByDescending(x => x.tarih).Where(x => x.kisi_id == id).ToList();
            ViewBag.dd = userDetail;
            ViewBag.a = a;
            ViewBag.end = end.AddDays(-1);
            ViewBag.d1 = end.ToString("yyyy-MM-dd");
            ViewBag.d2 = start.ToString("yyyy-MM-dd");
            if (a < 28)
            {
                //güne bağlı olarak listenin uzunluğunu belirler
                ViewBag.DataPoints1 = JsonConvert.SerializeObject(a);
            }
            else
            {
                ViewBag.DataPoints1 = JsonConvert.SerializeObject(25);
            }
            foreach (var item in ViewBag.dd)
            {
                for (int i = 0; i < a; i++)
                {
                    if (item.tarih.ToString("0:dd/MM/yyyy") == DateTime.Now.AddDays(-i).ToString("0:dd/MM/yyyy"))
                    {
                        if (item.value == 0 || item.value == 2)
                        {
                            if (item.value == 0)
                            {
                                giris[i] = item.zaman.ToString("hh\\:mm");
                            }
                            else
                            {
                                giris1[i] = item.zaman.ToString("hh\\:mm");
                            }
                            break;
                        }
                        else if (item.value == 1 || item.value == 3)
                        {
                            if (item.value == 1)
                            {
                                cikis[i] = item.zaman.ToString("hh\\:mm");
                            }
                            else
                            {
                                cikis1[i] = item.zaman.ToString("hh\\:mm");
                            }
                            break;
                        }
                    }
                }
            }
            for (int i = 0; i < a; i++)
            {
                if (giris[i] == null)
                {
                    giris[i] = "5555";
                }
                if (giris1[i] == null)
                {
                    giris1[i] = "5555";
                }
                if (cikis[i] == null)
                {
                    cikis[i] = "0";
                }
                if (cikis1[i] == null)
                {
                    cikis1[i] = "0";
                }
                if (int.Parse(giris[i].Replace(":", "")) > int.Parse(giris1[i].Replace(":", "")))
                {
                    giris[i] = giris1[i];
                }
                if (int.Parse(cikis[i].Replace(":", "")) < int.Parse(cikis1[i].Replace(":", "")))
                {
                    cikis[i] = cikis1[i];
                }
                if (giris[i] == "5555")
                {
                    giris[i] = null;
                }
                if (giris1[i] == "5555")
                {
                    giris1[i] = null;
                }
                if (cikis[i] == "0")
                {
                    cikis[i] = null;
                }
                if (cikis1[i] == "0")
                {
                    cikis1[i] = null;
                }
            }
            ViewBag.giris = giris;
            ViewBag.cikis = cikis;
            return View();
        }
        public ActionResult Excel(DateTime start, DateTime end, int id)
        {
            TimeSpan diff = end - start;
            int a = diff.Days;
            string[] giris = new string[a];
            string[] cikis = new string[a];
            using (var workbook = new XLWorkbook())
            {
                var userDetail = db.giriscikis.OrderByDescending(x => x.tarih).Where(x => x.kisi_id == id).ToList();
                var liste = db.personelgenel.Where(x => x.kart_no == id).FirstOrDefault();
                ViewBag.dd = userDetail;
                ViewBag.a = a;
                ViewBag.end = end.AddDays(-1);
                var worksheet = workbook.Worksheets.Add("Users");
                var rows = 3;
                var columnA = worksheet.Column("A");
                columnA.Width = 12;
                var columnB = worksheet.Column("B");
                columnB.Width = 12;
                var rows1 = worksheet.Row(1);
                rows1.Style.Font.Bold = true;
                var rows2 = worksheet.Row(2);
                rows2.Style.Font.Bold = true;
                var rows3 = worksheet.Row(6);
                rows3.Style.Font.Bold = true;
                var columnC = worksheet.Column("C");
                columnC.Width = 12;
                var columnD = worksheet.Column("D");
                columnD.Width = 12;
                var columnE = worksheet.Column("E");
                columnE.Width = 12;
                worksheet.Range("A1:E1").Merge();
                worksheet.Range("B4:C4").Merge();
                worksheet.Range("A1").Style.Font.FontSize = 18;
                worksheet.Range("A1").Style.Font.FontColor = XLColor.Red;
                //bu hücreye yazılan değerin ortalanmasına olanak sağlar
                worksheet.Range("A1:E1").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                worksheet.Range("A1").Value = "Personel Kişisel Bilgiler";
                worksheet.Cell(rows, 1).Style.Font.Bold = true;
                worksheet.Cell(rows, 1).Value = "Kart ID:";
                worksheet.Cell(rows, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                worksheet.Cell(rows, 2).Value = liste.kart_no;
                worksheet.Cell(rows + 1, 1).Style.Font.Bold = true;
                worksheet.Cell(rows + 1, 1).Value = "Adı Soyadı:";
                worksheet.Cell(rows + 1, 2).Value = liste.ad + " " + liste.soyad;
                rows = 6;
                worksheet.Cell(rows, 1).Value = "Tarih";
                worksheet.Cell(rows, 2).Value = "Giriş Saati";
                worksheet.Cell(rows, 3).Value = "Çıkış Saati";
                foreach (var item in ViewBag.dd)
                {
                    for (int i = 0; i < a; i++)
                    {
                        if (item.tarih.ToString("0:dd/MM/yyyy") == ViewBag.end.AddDays(-i).ToString("0:dd/MM/yyyy"))
                        {
                            if (item.value == 0)
                            {
                                giris[i] = item.zaman.ToString("hh\\:mm");
                                break;
                            }
                            else if (item.value == 1)
                            {
                                cikis[i] = item.zaman.ToString("hh\\:mm");
                                break;
                            }
                        }
                    }
                }
                for (int i = 0; i < a; i++)
                {
                    rows++;
                    worksheet.Cell(rows, 1).Value = ViewBag.end.AddDays(-i).ToString("dd/MM/yyyy");
                    worksheet.Cell(rows, 2).Value = giris[i];
                    worksheet.Cell(rows, 3).Value = cikis[i];
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", +id +
                        "_personel.xlsx");
                }
            }
        }
        public ActionResult Puandaj()
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
                      "Value", "Text", "").ToList(), "Value", "Text", yil.ToString());
            ViewBag.ay = new SelectList(new SelectList(
          Enumerable.Range(1, 12)
              .Select(r => new
              {
                  Text = new DateTime(1, r, 1).ToString("MM"),
                  Value = r.ToString()
              }),
          "Value", "Text", "").ToList(), "Value", "Text", ay.ToString());
            ViewBag.day = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            ViewBag.kisiler = db.personelgenel.Where(x => x.isdisable == 0).ToList();
            int j = db.personelgenel.Where(x => x.isdisable == 0).Count();
            string[,] giris = new string[j + 1, ViewBag.day + 1];
            string[,] cikis = new string[j + 1, ViewBag.day + 1];
            string[,] giris1 = new string[j + 1, ViewBag.day + 1];
            string[,] cikis1 = new string[j + 1, ViewBag.day + 1];
            int count = 0;
            ViewBag.dd = db.giriscikis.OrderBy(x => x.kisi_id).Where(x => x.tarih >= myDate).ToList();
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
                                    if (item1.value == 0 || item1.value == 2)
                                    {
                                        if (item1.value == 0)
                                        {
                                            giris[count, i] = item1.zaman.ToString("hh\\:mm");
                                        }
                                        else
                                        {
                                            giris1[count, i] = item1.zaman.ToString("hh\\:mm");
                                        }
                                        break;
                                    }
                                    else if (item1.value == 1 || item1.value == 3)
                                    {
                                        if (item1.value == 1)
                                        {
                                            cikis[count, i] = item1.zaman.ToString("hh\\:mm");
                                        }
                                        else
                                        {
                                            cikis1[count, i] = item1.zaman.ToString("hh\\:mm");
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            for (int i = 1; i <= j; i++)
            {
                for (int k = 1; k <= ViewBag.day; k++)
                {
                    if (giris[i, k] == null)
                    {
                        giris[i, k] = "5555";
                    }
                    if (giris1[i, k] == null)
                    {
                        giris1[i, k] = "5555";
                    }
                    if (cikis[i, k] == null)
                    {
                        cikis[i, k] = "0";
                    }
                    if (cikis1[i, k] == null)
                    {
                        cikis1[i, k] = "0";
                    }
                    if (int.Parse(giris[i, k].Replace(":", "")) > int.Parse(giris1[i, k].Replace(":", "")))
                    {
                        giris[i, k] = giris1[i, k];
                    }
                    if (int.Parse(cikis[i, k].Replace(":", "")) < int.Parse(cikis1[i, k].Replace(":", "")))
                    {
                        cikis[i, k] = cikis1[i, k];
                    }
                    if (giris[i, k] == "5555")
                    {
                        giris[i, k] = null;
                    }
                    if (giris1[i, k] == "5555")
                    {
                        giris1[i, k] = null;
                    }
                    if (cikis[i, k] == "0")
                    {
                        cikis[i, k] = null;
                    }
                    if (cikis1[i, k] == "0")
                    {
                        cikis1[i, k] = null;
                    }
                }
            }
            ViewBag.giris = giris;
            ViewBag.cikis = cikis;
            return View();
        }
        [HttpPost]
        public ActionResult Puandaj(int yil, int ay)
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
                      "Value", "Text", "").ToList(), "Value", "Text", yil.ToString());
            ViewBag.ay = new SelectList(new SelectList(
          Enumerable.Range(1, 12)
              .Select(r => new
              {
                  Text = new DateTime(1, r, 1).ToString("MM"),
                  Value = r.ToString()
              }),
          "Value", "Text", "").ToList(), "Value", "Text", ay.ToString());
            ViewBag.day = DateTime.DaysInMonth(yil, ay);
            ViewBag.kisiler = db.personelgenel.Where(x => x.isdisable == 0).ToList();
            int j = db.personelgenel.Where(x => x.isdisable == 0).Count();
            string[,] giris = new string[j + 1, ViewBag.day + 1];
            string[,] cikis = new string[j + 1, ViewBag.day + 1];
            string[,] giris1 = new string[j + 1, ViewBag.day + 1];
            string[,] cikis1 = new string[j + 1, ViewBag.day + 1];
            int count = 0;
            ViewBag.dd = db.giriscikis.OrderBy(x => x.kisi_id).Where(x => x.tarih >= myDate).ToList();
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
                                    if (item1.value == 0 || item1.value == 2)
                                    {
                                        if (item1.value == 0)
                                        {
                                            giris[count, i] = item1.zaman.ToString("hh\\:mm");
                                        }
                                        else
                                        {
                                            giris1[count, i] = item1.zaman.ToString("hh\\:mm");
                                        }
                                        break;
                                    }
                                    else if (item1.value == 1 || item1.value == 3)
                                    {
                                        if (item1.value == 1)
                                        {
                                            cikis[count, i] = item1.zaman.ToString("hh\\:mm");
                                        }
                                        else
                                        {
                                            cikis1[count, i] = item1.zaman.ToString("hh\\:mm");
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            for (int i = 1; i <= j; i++)
            {
                for (int k = 1; k <= ViewBag.day; k++)
                {
                    if (giris[i, k] == null)
                    {
                        giris[i, k] = "5555";
                    }
                    if (giris1[i, k] == null)
                    {
                        giris1[i, k] = "5555";
                    }
                    if (cikis[i, k] == null)
                    {
                        cikis[i, k] = "0";
                    }
                    if (cikis1[i, k] == null)
                    {
                        cikis1[i, k] = "0";
                    }
                    if (int.Parse(giris[i, k].Replace(":", "")) > int.Parse(giris1[i, k].Replace(":", "")))
                    {
                        giris[i, k] = giris1[i, k];
                    }
                    if (int.Parse(cikis[i, k].Replace(":", "")) < int.Parse(cikis1[i, k].Replace(":", "")))
                    {
                        cikis[i, k] = cikis1[i, k];
                    }
                    if (giris[i, k] == "5555")
                    {
                        giris[i, k] = null;
                    }
                    if (giris1[i, k] == "5555")
                    {
                        giris1[i, k] = null;
                    }
                    if (cikis[i, k] == "0")
                    {
                        cikis[i, k] = null;
                    }
                    if (cikis1[i, k] == "0")
                    {
                        cikis1[i, k] = null;
                    }
                }
            }
            ViewBag.giris = giris;
            ViewBag.cikis = cikis;
            return View();
        }
        public ActionResult PuandajExcel(int yil, int ay)
        {
            //gelen aya ait ilk günün versini almamızı sağlar
            var myDate = new DateTime(yil, ay, 1);
            ViewBag.day = DateTime.DaysInMonth(yil, ay);
            int fark = 31 - ViewBag.day;
            //hala çalışan personelleri listeye alır
            ViewBag.kisiler = db.personelgenel.Where(x => x.isdisable == 0).ToList();
            int j = db.personelgenel.Where(x => x.isdisable == 0).Count();
            string[,] giris = new string[j + 1, ViewBag.day + 1];
            string[,] cikis = new string[j + 1, ViewBag.day + 1];
            string[,] giris1 = new string[j + 1, ViewBag.day + 1];
            string[,] cikis1 = new string[j + 1, ViewBag.day + 1];
            int count = 0;
            ViewBag.dd = db.giriscikis.OrderBy(x => x.kisi_id).Where(x => x.tarih >= myDate).ToList();
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
                                    if (item1.value == 0 || item1.value == 2)
                                    {
                                        if (item1.value == 0)
                                        {
                                            giris[count, i] = item1.zaman.ToString("hh\\:mm");
                                        }
                                        else
                                        {
                                            giris1[count, i] = item1.zaman.ToString("hh\\:mm");
                                        }
                                        break;
                                    }
                                    else if (item1.value == 1 || item1.value == 3)
                                    {
                                        if (item1.value == 1)
                                        {
                                            cikis[count, i] = item1.zaman.ToString("hh\\:mm");
                                        }
                                        else
                                        {
                                            cikis1[count, i] = item1.zaman.ToString("hh\\:mm");
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 1; i <= j; i++)
            {
                for (int k = 1; k <= ViewBag.day; k++)
                {
                    if (giris[i, k] == null)
                    {
                        giris[i, k] = "5555";
                    }
                    if (giris1[i, k] == null)
                    {
                        giris1[i, k] = "5555";
                    }
                    if (cikis[i, k] == null)
                    {
                        cikis[i, k] = "0";
                    }
                    if (cikis1[i, k] == null)
                    {
                        cikis1[i, k] = "0";
                    }
                    if (int.Parse(giris[i, k].Replace(":", "")) > int.Parse(giris1[i, k].Replace(":", "")))
                    {
                        giris[i, k] = giris1[i, k];
                    }
                    if (int.Parse(cikis[i, k].Replace(":", "")) < int.Parse(cikis1[i, k].Replace(":", "")))
                    {
                        cikis[i, k] = cikis1[i, k];
                    }
                    if (giris[i, k] == "5555")
                    {
                        giris[i, k] = null;
                    }
                    if (giris1[i, k] == "5555")
                    {
                        giris1[i, k] = null;
                    }
                    if (cikis[i, k] == "0")
                    {
                        cikis[i, k] = null;
                    }
                    if (cikis1[i, k] == "0")
                    {
                        cikis1[i, k] = null;
                    }
                }
            }
            // hazırda var olan dosyanın işlenmesini ve üzerine veri yazılmasını sağlar
            string path = Server.MapPath("~") + @"document\puantaj.xlsx";
            FileInfo file = new FileInfo(path);
            ExcelPackage p = new ExcelPackage(file);
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            ExcelWorksheet ws = p.Workbook.Worksheets["Sayfa2"];
            if (file.Exists)
            {
                //Exceldeki fazla günleri siler
                for (int s = 0; s < fark; s++)
                {
                    ws.DeleteColumn(ViewBag.day + 4);
                }
                //her ay için tarihleri yazar
                for (int i = 0; i < ViewBag.day; i++)
                {
                    ws.Cells[1, i + 4].Value = myDate.AddDays(i).ToShortDateString();
                    int dt = (int)myDate.AddDays(i).DayOfWeek;
                    if (dt == 6 || dt == 0)
                    {
                        ws.Cells[1, i + 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                    }
                }
                int k = 2;
                foreach (var item in ViewBag.kisiler)
                {
                    ws.Cells[k, 1].Value = item.kart_no;
                    ws.Cells[k, 2].Value = item.ad;
                    ws.Cells[k, 3].Value = item.soyad;
                    k += 2;
                }
                for (int i = 2; i <= j * 2; i++)
                {
                    for (int m = 4; m <= ViewBag.day + 3; m++)
                    {
                        if (i % 2 == 0)
                        {
                            ws.Cells[i, m].Value = giris[i / 2, m - 3];
                        }
                        else
                        {
                            ws.Cells[i, m].Value = cikis[i / 2, m - 3];
                        }
                    }
                }
            }
            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=" + DateTime.Now + "_giriscikis.xlsx");
                p.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
            ViewBag.giris = giris;
            ViewBag.cikis = cikis;
            return View();
        }
    }
}