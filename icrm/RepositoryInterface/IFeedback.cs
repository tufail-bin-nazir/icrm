﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using icrm.Models;
using PagedList.Mvc;
using PagedList;

namespace icrm.RepositoryInterface
{
    interface IFeedback : IDisposable
    {
        void Save(Feedback feedback);
        Feedback Find(string id);
        IPagedList<Feedback> Pagination(int pageIndex, int pageSize,string userId);
        IPagedList<Feedback> getAllAssigned(int pageIndex, int pageSize);
        IEnumerable<Feedback> getAllAssigned();
        IEnumerable<Feedback> GetAllAssigned();
        IPagedList<Feedback> search(string d1,string d2,string status,string id,int pageIndex, int pageSize);
        IEnumerable<Feedback> getAll();
        IPagedList<Feedback> getAll(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllOpenWithDepartment(string id, int pageIndex, int pageSize);
        IPagedList<Feedback> OpenWithoutDepart(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllResolved(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllResponded(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllClosed(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllRespondedWithDepartment(string id, int pageIndex, int pageSize);
        IEnumerable<Feedback> getAllRespondedWithDepartment(string id);
        IEnumerable<Feedback> getAllOpen();
        IEnumerable<Feedback> getAllClosed();
        IEnumerable<Feedback> getAllResolved();
        IEnumerable<Feedback> getAllResponded();
        IEnumerable<Feedback> GetAllResponded();
        IEnumerable<Feedback> GetAllRespondedMobile();
        IEnumerable<Feedback> OpenWithoutDepart();
        IEnumerable<Feedback> GETAllClosed();

        IEnumerable<Feedback> getRespondedDepartmenet();


        IEnumerable<Feedback> searchlist(DateTime d1, DateTime d2);
        IEnumerable<Feedback> searchHR(string d1, string d2, string status);
        IPagedList<Feedback> searchHR(string v1, string v2, string status, int pageIndex, int pageSize);
        IPagedList<Feedback> getAllRejected(int pageIndex, int pageSize);
        IPagedList<Feedback> getAllOpen(int pageIndex, int pageSize);


        List<Comments> getCOmments(string id);

        List<Category> getCategories(Int32 deptId,Int32 type);
        List<SubCategory> getSubCategories(Int32 categoryId, Int32 typeId);

        List<string> getEmails();
        IPagedList<Feedback> getListBasedOnType(int pageIndex, int pageSize, string type);
        List<Department> getDepartmentsOnType(string fORWARD);
        List<Priority> getPriorties();
        List<FeedBackType> getFeedbackTypes();
        ApplicationUser getEmpDetails(string id);

        IEnumerable<Feedback> GetListBasedOnType(string type);

        IEnumerable<Feedback> getAllByDept(string id);
        IEnumerable<Feedback> getAllOpenByDept(string id);
        IEnumerable<Feedback> getAllClosedByDept(string id);
        IEnumerable<Feedback> getAllResolvedByDept(string id);

        IEnumerable<Feedback> getAllOpenMobile();

        IEnumerable<Feedback> GetAllResolvedmobile();
        ApplicationUser getEscalationUser(int? departmentId, int? categoryId);
        IEnumerable<Medium> getSourceList();
        ApplicationUser getOperationsEscalationUser(int? costCenterId);

        IEnumerable<Feedback> chartsFeedbackTypes(string d1, string d2, string type);
        IEnumerable<Feedback> chartsFeedbackAll(string d1, string d2);


       
        IEnumerable<Feedback> chartsFeedbackSource(string d1, string d2, string source);
        IEnumerable<Feedback> chartsFeedbackDepartment(string d1, string d2, string department);
        IEnumerable<Feedback> chartsFeedbackPosition(string d1, string d2, string position);
        IEnumerable<Feedback> chartsFeedbackNationalitySaudi(string d1, string d2);
        IEnumerable<Feedback> chartsFeedbackNationalityExpatriates(string d1, string d2);
        IEnumerable<Feedback> chartsFeedbackSatisfaction(string d1, string d2, string satisfaction);
        IEnumerable<Feedback> chartsFeedbackEscalation(string d1, string d2, string escalation);

        IEnumerable<Feedback> chartsFeedbackSalaryTimeSheet(string d1, string d2);
        IEnumerable<Feedback> chartsFeedbackHousing(string d1, string d2);
        IEnumerable<Feedback> chartsFeedbackPoorTreatment(string d1, string d2);
        IEnumerable<Feedback> chartsFeedbackAccomodationSupplies(string d1, string d2);
        IEnumerable<Feedback> chartsFeedbackSalaryIssuesReasons(string d1, string d2, string salaryissuesreasons);
        string[] chartsFeedbackMostFrequentLocations(string d1, string d2);
        IEnumerable<Feedback> chartsFeedbackRegion(string d1, string d2, string region);

    }
}
