using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
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
                            context.Configuration.AutoDetectChangesEnabled = false;
                            context.Configuration.ValidateOnSaveEnabled = false;

                            ApplicationUser user = new ApplicationUser();
                            user.EmployeeId = Convert.ToInt32(row.Cell(1).Value.ToString());
                            user.FirstName = row.Cell(2).Value.ToString();
                            user.MiddleName = row.Cell(3).IsEmpty() ? null : row.Cell(3).Value.ToString();
                            user.LastName = row.Cell(4).Value.ToString();
                            user.UserName ="user"+x+"@gmail.com";

                            var departmentname = row.Cell(5).Value.ToString();
                            Department department = context.Departments.Where(m => m.name == departmentname).SingleOrDefault();
                            if (department == null)
                            {
                                Department dp = new Department();
                                dp.name = departmentname;
                                //dp.departmntNumber = departmentId;
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


                            var costcentercode = row.Cell(6).IsEmpty()? null : row.Cell(6).Value.ToString();
                            var costcentername = row.Cell(7).Value.ToString();
                            CostCenter costCenter = context.CostCenters.Where(m => m.name == costcentername).SingleOrDefault();
                            if (costCenter == null)
                            {
                                CostCenter cs = new CostCenter();
                                cs.name = costcentername;
                                cs.CostCenterCode = costcentercode;
                                context.CostCenters.Add(cs);
                                context.SaveChanges();
                                user.CostCenterId = cs.Id;
                            }
                            else
                            {
                                user.CostCenterId = costCenter.Id;
                            }


                           // user.EmployeeHireDate = row.Cell(8).GetDateTime();
                            
                            var nationalityname = row.Cell(9).Value.ToString();
                            Nationality nationality = context.Nationalities.Where(m => m.name == nationalityname).SingleOrDefault();
                            if (nationality == null)
                            {
                                Nationality ps = new Nationality();
                                ps.name = nationalityname;
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

                           // user.EmployeeFireDate = row.Cell(10).GetDateTime();

                            user.EmployeeStatus = row.Cell(11).IsEmpty() ? null: row.Cell(11).Value.ToString();


                            var eventreason = row.Cell(12).Value.ToString();
                            EventReason eventReason = context.EventReasons.Where(m => m.name == eventreason).SingleOrDefault();
                            if (eventReason == null)
                            {
                                EventReason ev = new EventReason();
                                ev.name = eventreason;
                                context.EventReasons.Add(ev);
                                context.SaveChanges();
                                user.EventReasonId = ev.Id;
                                //context.SaveChanges();
                            }
                            else
                            {
                                user.EventReasonId = eventReason.Id;
                                //context.SaveChanges();
                            }

                            var locationId = row.Cell(13).Value.ToString();
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

                            var locationgroupId = row.Cell(14).Value.ToString();
                            LocationGroup locationgrp = context.LocationGroups.Where(m => m.name == locationgroupId).SingleOrDefault();
                            if (locationgrp == null)
                            {
                                LocationGroup lc = new LocationGroup();
                                lc.name = locationgroupId;
                                context.LocationGroups.Add(lc);
                                context.SaveChanges();
                                user.LocationGroupId = lc.Id;
                                //context.SaveChanges();
                            }
                            else
                            {
                                user.LocationGroupId = locationgrp.Id;
                                // context.SaveChanges();
                            }


                            var jobtitlename = row.Cell(15).Value.ToString();
                            JobTitle jobTitle = context.JobTitles.Where(m => m.name == jobtitlename).SingleOrDefault();
                            if (jobTitle == null)
                            {
                                JobTitle jb = new JobTitle();
                                jb.name = jobtitlename;
                                context.JobTitles.Add(jb);
                                context.SaveChanges();
                                user.JobTitleId = jb.Id;
                                //context.SaveChanges();
                            }
                            else {
                                user.JobTitleId = jobTitle.Id;
                               // context.SaveChanges();
                            }

                          
                            user.GOSINumber = row.Cell(16).IsEmpty() ? null : row.Cell(16).Value.ToString();

                            user.saudiNationalId = row.Cell(17).IsEmpty() ? null : row.Cell(17).Value.ToString();

                            user.saudiResidentialId = row.Cell(18).IsEmpty() ? null : row.Cell(18).Value.ToString();

                            var employeeclass = row.Cell(19).Value.ToString();
                            EmployeeClass employeeClass = context.employeeClasses.Where(m => m.name == employeeclass).SingleOrDefault();
                            if (employeeClass == null)
                            {
                                EmployeeClass ec = new EmployeeClass();
                                ec.name = employeeclass;
                                context.employeeClasses.Add(ec);
                                context.SaveChanges();
                                user.EmployeeClassId = ec.Id;
                                //context.SaveChanges();
                            }
                            else
                            {
                                user.EmployeeClassId = employeeClass.Id;
                                // context.SaveChanges();
                            }

                            var ethincity = row.Cell(20).Value.ToString();
                            Ethnicity ethnicity = context.Ethnicities.Where(m => m.name == ethincity).SingleOrDefault();
                            if (ethnicity == null)
                            {
                                Ethnicity ec = new Ethnicity();
                                ec.name = ethincity;
                                context.Ethnicities.Add(ec);
                                context.SaveChanges();
                                user.EthincityId = ec.Id;
                                //context.SaveChanges();
                            }
                            else
                            {
                                user.EthincityId = ethnicity.Id;
                                // context.SaveChanges();
                            }

                            user.HasSwipeAccess = row.Cell(21).Value.ToString().Equals("No Swipe Access") ? false : true;

                            user.bussinessPhoneNumber = row.Cell(22).IsEmpty() ? null : row.Cell(22).Value.ToString();

                            user.homePhoneNumber = row.Cell(23).IsEmpty() ? null : row.Cell(23).Value.ToString();

                            user.cellPhoneNumber = row.Cell(24).IsEmpty() ? null : row.Cell(24).Value.ToString(); ;

                            user.otherPhoneNumber = row.Cell(25).IsEmpty() ? null : row.Cell(25).Value.ToString();

                            var payscalename = row.Cell(26).Value.ToString();
                            PayScaleType payscale = context.PayScaleTypes.Where(m => m.name == payscalename).SingleOrDefault();
                            if (payscale == null)
                            {
                                PayScaleType ps = new PayScaleType();
                                ps.name = payscalename;
                                context.PayScaleTypes.Add(ps);
                                context.SaveChanges();
                                user.PayScaleTypeId = ps.Id;
                                //context.SaveChanges();
                            }
                            else
                            {
                                user.PayScaleTypeId = payscale.Id;
                                //context.SaveChanges();
                            }

                            var religionname = row.Cell(27).Value.ToString();
                            Religion religion = context.Religions.Where(m => m.name == religionname).SingleOrDefault();
                            if (religion == null)
                            {
                                Religion ps = new Religion();
                                ps.name = religionname.Equals(null)? "Others": religionname;
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

                            user.personalInfoString = row.Cell(28).IsEmpty() ? null : row.Cell(28).Value.ToString(); ;

                            //user.DOB = row.Cell(29).GetDateTime();


                            var gendername = row.Cell(30).Value.ToString();
                            Gender gender = context.Genders.Where(m => m.name == gendername).SingleOrDefault();
                            if (gender == null)
                            {
                                Gender gn = new Gender();
                                gn.name = gendername;
                                context.Genders.Add(gn);
                                context.SaveChanges();
                                user.GenderId = gn.Id;
                                //context.SaveChanges();
                            }
                            else
                            {
                                user.GenderId = gender.Id;
                                //context.SaveChanges();
                            }

                            user.otherEmail = row.Cell(31).IsEmpty() ? null : row.Cell(31).Value.ToString(); 

                            user.bussinessEmail = row.Cell(32).IsEmpty() ? null : row.Cell(32).Value.ToString(); 

                            user.personalEmail = row.Cell(33).IsEmpty() ? null : row.Cell(33).Value.ToString(); 


                            var employertypename = row.Cell(34).Value.ToString();
                            EmployerType employerType = context.employerTypes.Where(m => m.name == employertypename).SingleOrDefault();
                            if (employerType == null)
                            {
                                EmployerType ty = new EmployerType();
                                ty.name = employertypename;
                                context.employerTypes.Add(ty);
                                context.SaveChanges();
                                user.EmployerTypeId = ty.Id;
                                //context.SaveChanges();
                            }
                            else
                            {
                                user.EmployerTypeId = employerType.Id;
                                //context.SaveChanges();
                            }

                            var vendorname = row.Cell(35).Value.ToString();
                            Vendor vendor = context.vendors.Where(m => m.name == vendorname).SingleOrDefault();
                            if (vendor == null)
                            {
                                Vendor vn = new Vendor();
                                vn.name = vendorname;
                                context.vendors.Add(vn);
                                context.SaveChanges();
                                user.VendorId = vn.Id;
                                //context.SaveChanges();
                            }
                            else
                            {
                                user.VendorId = vendor.Id;
                                //context.SaveChanges();
                            }

                            user.outSourcedId = row.Cell(36).IsEmpty() ? null : row.Cell(36).Value.ToString();

                            var bandname = row.Cell(37).Value.ToString();
                            Band band = context.bands.Where(m => m.name == bandname).SingleOrDefault();
                            if (band == null)
                            {
                                Band bnd = new Band();
                                bnd.name = bandname;
                                context.bands.Add(bnd);
                                context.SaveChanges();
                                user.BandId = bnd.Id;
                                //context.SaveChanges();
                            }
                            else
                            {
                                user.BandId = band.Id;
                                //context.SaveChanges();
                            }
                             context.Users.Add(user);
                             x++;
                        }
                        try
                        {
                            context.SaveChanges();
                        }
                        catch (DbEntityValidationException ex)
                        {
                            Debug.WriteLine(ex.Message);
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
