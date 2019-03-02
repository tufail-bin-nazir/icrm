using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                        catch (Exception ex)
                        {
                            ex.StackTrace.ToArray();
                            ModelState.AddModelError(String.Empty, "sheet1 not found!");
                            return View();
                        }
                        WorkSheet.FirstRow().Delete();//if you want to remove ist row

                        var x = 1;
                        foreach (var row in WorkSheet.RowsUsed())
                        {
                            
                            context.Configuration.ProxyCreationEnabled = false;


                            ApplicationUser user;
                            int employeeID = Convert.ToInt32(row.Cell(1).Value.ToString());
                            if (context.Users.Where(m => m.EmployeeId == employeeID).SingleOrDefault() != null)
                            {

                                user = context.Users.Where(m => m.EmployeeId == employeeID).SingleOrDefault();
                                user.FirstName = row.Cell(2).Value.ToString();
                                user.MiddleName = row.Cell(3).IsEmpty() ? null : row.Cell(3).Value.ToString();
                                user.LastName = row.Cell(4).Value.ToString();
                                user.UserName = Guid.NewGuid().ToString("n").Substring(0, 8) + "@gmail.com";

                                var jobtitlename = row.Cell(5).Value.ToString();
                                JobTitle jobTitle = context.JobTitles.Where(m => m.name == jobtitlename).SingleOrDefault();
                                if (jobTitle == null)
                                {
                                    JobTitle jb = new JobTitle();
                                    jb.name = jobtitlename;
                                    context.JobTitles.Add(jb);
                                    context.SaveChanges();
                                    user.JobTitle = null;
                                    user.JobTitleId = jb.Id;
                                    //context.SaveChanges();
                                }
                                else
                                {

                                    user.JobTitle = null;
                                    user.JobTitleId = jobTitle.Id;
                                    // context.SaveChanges();
                                }

                                var departmentname = row.Cell(6).Value.ToString().Contains("Operations") ? "Operations" : row.Cell(6).Value.ToString();
                                Department department = context.Departments.Where(m => m.name == departmentname).SingleOrDefault();
                                if (department == null)
                                {
                                    Department dp = new Department();
                                    dp.name = departmentname;
                                    //dp.departmntNumber = departmentId;
                                    context.Departments.Add(dp);
                                    context.SaveChanges();
                                    user.Department = null; 
                                    user.DepartmentId = dp.Id;
                                    //context.SaveChanges();
                                }
                                else
                                {
                                    user.Department =null;
                                    user.DepartmentId = department.Id;
                                    // context.SaveChanges();
                                }


                                var costcentercode = row.Cell(8).IsEmpty() ? null : row.Cell(8).Value.ToString();
                                var costcentername = row.Cell(7).Value.ToString();
                                CostCenter costCenter = context.CostCenters.Where(m => m.name == costcentername).SingleOrDefault();
                                if (costCenter == null)
                                {
                                    CostCenter cs = new CostCenter();
                                    cs.name = costcentername;
                                    cs.CostCenterCode = costcentercode;
                                    context.CostCenters.Add(cs);
                                    context.SaveChanges();
                                    user.CostCenter = null;
                                    user.CostCenterId = cs.Id;
                                }
                                else
                                {
                                    user.CostCenter = null;
                                    user.CostCenterId = costCenter.Id;
                                }

                                var nationalityname = row.Cell(11).Value.ToString();
                                Nationality nationality = context.Nationalities.Where(m => m.name == nationalityname).SingleOrDefault();
                                if (nationality == null)
                                {
                                    Nationality ps = new Nationality();
                                    ps.name = nationalityname;
                                    context.Nationalities.Add(ps);
                                    context.SaveChanges();
                                    user.Nationality = null;
                                    user.NationalityId = ps.Id;
                                    //context.SaveChanges();
                                }
                                else
                                {
                                    user.Nationality = null;
                                    user.NationalityId = nationality.Id;
                                    //context.SaveChanges();
                                }

                                var locationId = row.Cell(15).Value.ToString();
                                Location location = context.Locations.Where(m => m.name == locationId).SingleOrDefault();
                                if (location == null)
                                {
                                    Location lc = new Location();
                                    lc.name = locationId;
                                    context.Locations.Add(lc);
                                    context.SaveChanges();
                                    user.Location = null;
                                    user.LocationId = lc.Id;
                                    //context.SaveChanges();
                                }
                                else
                                {
                                    user.Location = null;
                                    user.LocationId = location.Id;
                                    // context.SaveChanges();
                                }

                                var locationgroupId = row.Cell(16).Value.ToString();
                                LocationGroup locationgrp = context.LocationGroups.Where(m => m.name == locationgroupId).SingleOrDefault();
                                if (locationgrp == null)
                                {
                                    LocationGroup lc = new LocationGroup();
                                    lc.name = locationgroupId;
                                    context.LocationGroups.Add(lc);
                                    context.SaveChanges();
                                    user.LocationGroup = null;
                                    user.LocationGroupId = lc.Id;
                                    //context.SaveChanges();
                                }
                                else
                                {
                                    user.LocationGroup = null;
                                    user.LocationGroupId = locationgrp.Id;
                                    // context.SaveChanges();
                                }

                                var gendername = row.Cell(17).Value.ToString();
                                Gender gender = context.Genders.Where(m => m.name == gendername).SingleOrDefault();
                                if (gender == null)
                                {
                                    Gender gn = new Gender();
                                    gn.name = gendername;
                                    context.Genders.Add(gn);
                                    context.SaveChanges();
                                    user.Gender = null;
                                    user.GenderId = gn.Id;
                                    //context.SaveChanges();
                                }
                                else
                                {
                                    user.Gender = null;
                                    user.GenderId = gender.Id;
                                    //context.SaveChanges();
                                }

                                var employertypename = row.Cell(18).Value.ToString();
                                EmployerType employerType = context.employerTypes.Where(m => m.name == employertypename).SingleOrDefault();
                                if (employerType == null)
                                {
                                    EmployerType ty = new EmployerType();
                                    ty.name = employertypename;
                                    context.employerTypes.Add(ty);
                                    context.SaveChanges();
                                    user.EmployerType = null;
                                    user.EmployerTypeId = ty.Id;
                                    //context.SaveChanges();
                                }
                                else
                                {
                                    user.EmployerType = null;
                                    user.EmployerTypeId = employerType.Id;
                                    //context.SaveChanges();
                                }

                                var vendorname = row.Cell(19).Value.ToString();
                                Vendor vendor = context.vendors.Where(m => m.name == vendorname).SingleOrDefault();
                                if (vendor == null)
                                {
                                    Vendor vn = new Vendor();
                                    vn.name = vendorname;
                                    context.vendors.Add(vn);
                                    context.SaveChanges();
                                    user.Vendor = null;
                                    user.VendorId = vn.Id;
                                    //context.SaveChanges();
                                }
                                else
                                {
                                    user.Vendor = null;
                                    user.VendorId = vendor.Id;
                                    //context.SaveChanges();
                                }


                                user.saudiResidentialId = row.Cell(21).IsEmpty() ? null : row.Cell(21).Value.ToString();

                                user.homePhoneNumber = row.Cell(23).IsEmpty() ? null : row.Cell(23).Value.ToString();

                                user.otherPhoneNumber = row.Cell(24).IsEmpty() ? null : row.Cell(24).Value.ToString();

                                user.bussinessPhoneNumber = row.Cell(26).IsEmpty() ? null : row.Cell(26).Value.ToString();


                                var ethincity = row.Cell(27).Value.ToString();
                                Ethnicity ethnicity = context.Ethnicities.Where(m => m.name == ethincity).SingleOrDefault();
                                if (ethnicity == null)
                                {
                                    Ethnicity ec = new Ethnicity();
                                    ec.name = ethincity;
                                    context.Ethnicities.Add(ec);
                                    context.SaveChanges();
                                    user.Ethnicity = null;
                                    user.EthincityId = ec.Id;
                                    //context.SaveChanges();
                                }
                                else
                                {
                                    user.Ethnicity = null;
                                    user.EthincityId = ethnicity.Id;
                                    // context.SaveChanges();
                                }


                                // user.cellPhoneNumber = row.Cell(24).IsEmpty() ? null : row.Cell(24).Value.ToString(); ;
                                context.Entry(user).State = EntityState.Modified;

                               

                            }

                            else
                            {
                                user = new ApplicationUser();
                                user.EmployeeId = Convert.ToInt32(row.Cell(1).Value.ToString());
                                user.FirstName = row.Cell(2).Value.ToString();
                                user.MiddleName = row.Cell(3).IsEmpty() ? null : row.Cell(3).Value.ToString();
                                user.LastName = row.Cell(4).Value.ToString();
                                user.UserName = Guid.NewGuid().ToString("n").Substring(0, 8) + "@gmail.com";

                                var jobtitlename = row.Cell(5).Value.ToString();
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
                                else
                                {
                                    user.JobTitleId = jobTitle.Id;
                                    // context.SaveChanges();
                                }

                                var departmentname = row.Cell(6).Value.ToString().Contains("Operations") ? "Operations" : row.Cell(6).Value.ToString();
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


                                var costcentercode = row.Cell(8).IsEmpty() ? null : row.Cell(8).Value.ToString();
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

                                var nationalityname = row.Cell(11).Value.ToString();
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

                                var locationId = row.Cell(15).Value.ToString();
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

                                var locationgroupId = row.Cell(16).Value.ToString();
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

                                var gendername = row.Cell(17).Value.ToString();
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

                                var employertypename = row.Cell(18).Value.ToString();
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

                                var vendorname = row.Cell(19).Value.ToString();
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


                                user.saudiResidentialId = row.Cell(21).IsEmpty() ? null : row.Cell(21).Value.ToString();

                                user.homePhoneNumber = row.Cell(23).IsEmpty() ? null : row.Cell(23).Value.ToString();

                                user.otherPhoneNumber = row.Cell(24).IsEmpty() ? null : row.Cell(24).Value.ToString();

                                user.bussinessPhoneNumber = row.Cell(26).IsEmpty() ? null : row.Cell(26).Value.ToString();


                                var ethincity = row.Cell(27).Value.ToString();
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
