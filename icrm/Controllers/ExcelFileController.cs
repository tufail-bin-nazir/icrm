using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using icrm.Models;

namespace icrm.Controllers
{
    public class ExcelFileController : Controller
    {
        ApplicationDbContext context = new ApplicationDbContext();
        public ActionResult Index()
        {
            UploadFile UploadFile = new UploadFile();
            return View(UploadFile);
        }

        [HttpPost]
        public ActionResult Index(UploadFile UploadFile)
        {
            if (ModelState.IsValid)
            {

                if (UploadFile.ExcelFile.ContentLength > 0)
                {
                    if (UploadFile.ExcelFile.FileName.EndsWith(".xlsx") || UploadFile.ExcelFile.FileName.EndsWith(".xls"))
                    {
                        XLWorkbook Workbook;
                        try
                            {
                            Workbook = new XLWorkbook(UploadFile.ExcelFile.InputStream);
                        }
                            catch (Exception ex)
                        {
                            ModelState.AddModelError(String.Empty, $"Check your file. {ex.Message}");
                            return View();
                        }
                        IXLWorksheet WorkSheet = null;
                       try
                            {
                            WorkSheet = Workbook.Worksheet(1);

                        }
                            catch(Exception ex)
                        {
                            ex.StackTrace.ToArray();
                            ModelState.AddModelError(String.Empty, "sheet1 not found!");
                            return View();
                        }
                        WorkSheet.FirstRow().Delete();//if you want to remove ist row

                        var x = 1;
                        foreach (var row in WorkSheet.RowsUsed())
                        {
                            ApplicationUser user = new ApplicationUser();
                            user.EmployeeId = Convert.ToInt32(row.Cell(1).Value.ToString());
                            user.LastName = row.Cell(2).Value.ToString();
                            user.FirstName = row.Cell(3).Value.ToString();
                            user.UserName = "user"+ x +"@user.com";


                            var jobtitlename = row.Cell(4).Value.ToString();
                            JobTitle jobTitle = context.JobTitles.Where(m => m.name == jobtitlename).SingleOrDefault();
                            if (jobTitle == null)
                            {
                                JobTitle jb = new JobTitle();
                                jb.name = row.Cell(4).Value.ToString();
                                context.JobTitles.Add(jb);
                                context.SaveChanges();
                                user.JobTitleId = jb.Id;
                                //context.SaveChanges();
                            }
                            else {
                                user.JobTitleId = jobTitle.Id;
                               // context.SaveChanges();
                            }




                            var departmentId = Convert.ToInt32( row.Cell(5).Value.ToString().Substring(0, 8));
                            Department department = context.Departments.Where(m => m.departmntNumber == departmentId).SingleOrDefault();
                            if (department == null)
                            {
                                Department dp = new Department();
                                dp.name = row.Cell(5).Value.ToString().Remove(0, 9);
                                dp.departmntNumber = departmentId;
                                context.Departments.Add(dp);
                                context.SaveChanges();
                                user.DepartmentId = dp.Id;
                                //context.SaveChanges();
                            }
                            else
                            {
                                user.DepartmentId = department.Id;
                               // context.SaveChanges();
                            }


                            var locationId = row.Cell(6).Value.ToString().Substring(0, 7);
                            Location location = context.Locations.Where(m => m.name == locationId).SingleOrDefault();
                            if (location == null)
                            {
                                Location lc = new Location();
                                lc.name = locationId;
                                context.Locations.Add(lc);
                                context.SaveChanges();
                                user.LocationId = lc.Id;
                                //context.SaveChanges();
                            }
                            else
                            {
                                user.LocationId = location.Id;
                                // context.SaveChanges();
                            }


                            var sublocation = row.Cell(6).Value.ToString().Remove(0,8);
                            SubLocation subLocation = context.SubLocations.Where(m => m.name == sublocation).SingleOrDefault();
                            if (subLocation == null)
                            {
                                SubLocation sl = new SubLocation();
                                sl.name = sublocation;
                                sl.LocationId = context.Locations.Where(m => m.name == locationId).SingleOrDefault().Id;
                                context.SubLocations.Add(sl);
                                context.SaveChanges();
                                user.SubLocationId = sl.Id;
                                //context.SaveChanges();
                            }
                            else
                            {
                                user.SubLocationId = subLocation.Id;
                                // context.SaveChanges();
                            }


                            var payscalename = row.Cell(7).Value.ToString();
                            PayScaleType payScaleType = context.PayScaleTypes.Where(m => m.name == payscalename).SingleOrDefault();
                            if (payScaleType == null)
                            {
                                PayScaleType ps = new PayScaleType();
                                ps.name = row.Cell(7).Value.ToString();
                                context.PayScaleTypes.Add(ps);
                                context.SaveChanges();
                                user.PayScaleTypeId = ps.Id;
                                //context.SaveChanges();
                            }
                            else
                            {
                                user.PayScaleTypeId = payScaleType.Id;
                                //context.SaveChanges();
                            }

                           

                            var religionname = row.Cell(9).Value.ToString();
                            Religion religion = context.Religions.Where(m => m.name == religionname).SingleOrDefault();
                            if (religion == null)
                            {
                                Religion ps = new Religion();
                                ps.name = row.Cell(9).Value.ToString();
                                context.Religions.Add(ps);
                                context.SaveChanges();
                                user.ReligionId = ps.Id;
                                //context.SaveChanges();
                            }
                            else
                            {
                                user.ReligionId = religion.Id;
                                //context.SaveChanges();
                            }

                            var nationalityname = row.Cell(10).Value.ToString();
                            Nationality nationality = context.Nationalities.Where(m => m.name == nationalityname).SingleOrDefault();
                            if (nationality == null)
                            {
                                Nationality ps = new Nationality();
                                ps.name = row.Cell(10).Value.ToString();
                                context.Nationalities.Add(ps);
                                context.SaveChanges();
                                user.NationalityId = ps.Id;
                                //context.SaveChanges();
                            }
                            else
                            {
                                user.NationalityId = nationality.Id;
                                //context.SaveChanges();
                            }
                            
                            context.Users.Add(user);
                            context.SaveChanges();
                            x++;
                        }

                       

                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, "Only .xlsx and .xls files are allowed");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Not a valid file");
                    return View();
                }
            }
            return View();
        }
    }
}
