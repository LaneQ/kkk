//Namespace
var ServiceProviders = ServiceProviders || {};

//Job constructor
ServiceProviders.Job = function () {
    this.Id = ko.observable(-1);
    this.PropertyId = ko.observable();
    this.PropertyName = ko.observable();
    this.ProviderId = ko.observable(23);
    this.ProviderName = ko.observable();
    this.JobStartDate = ko.observable((new Date()).toISOString().split("T")[0]);
    this.JobEndDate = ko.observable((new Date()).toISOString().split("T")[0]);
    this.JobStatusId = ko.observable();
    this.JobStatusName = ko.observable();
    this.JobDescription = ko.observable();
    this.CreatedOn = ko.observable();
    this.UpdatededOn = ko.observable();
    this.PaymentAmount = ko.observable();
    //this.PhysicalAddress = ko.observable();
};

ServiceProviders.JobViewModel = function (jobListData,propertyData) {
    var self = this;
    debugger;
    // JobList model and Job model
    self.jobList = ko.mapping.fromJS(jobListData);
    self.job = ko.mapping.fromJS(new ServiceProviders.Job());

    // Properties 
    self.isEditing = ko.observable();
    self.isDetail = ko.observable(false);
    self.modalStatus = ko.observable();
    self.action; // Used for restricting user action
    self.isLoaded = ko.observable(false); // Used for preventing flicker
    self.applyJob = ko.observable();

    self.properties = ko.mapping.fromJS(propertyData); // Dropdown list for properties
    self.jobStatus = [
        { Id: 1, Code: "Finished", Name: "Finished" },
        { Id: 2, Code: "Inprocess", Name: "Inprocess" },
        { Id: 3, Code: "Pending", Name: "Pending" }, ] //Dropdown list for job status

    // behaviors
    self.addJob = function () {
        self.isEditing(false);
        self.modalStatus("Create new job");
        self.isDetail(false);
        ko.mapping.fromJS(new ServiceProviders.Job(), self.job);
        //self.job(new ServiceProviders.Job());
    };

    self.editJob = function (job) {
        self.isEditing(true);
        self.modalStatus("Edit job");
        self.isDetail(false);
        //ServiceProviders.mapJobToJobModel(job, self.job());
        ko.mapping.fromJS(job, self.job);
    };

    self.save = function () {
        console.log(self.job);
        if (self.isEditing()) {
            ServiceProviders.editJob(self.job, self.jobList);
        } else {
            ServiceProviders.createJob(self.job, self.jobList);
        }
    }

    self.deleteJob = function (job) {
        // After deleting, fetch the data of current page from server again
        $.post("ServiceProviders/Home/Delete", { id: job.Id() }, function () {
            ServiceProviders.fetchDataOfCurrentPage(self.jobList)
        });
    }
    self.deleteJobQuote = function (job) {
        // After deleting, fetch the data of current page from server again
        $.post("ServiceProviders/Home/DeleteJobQuote", { id: job.Id }, function () {
            ServiceProviders.fetchDataOfCurrentPage(self.jobList)
        });
    }


    //self.quoteJob = function (job) {
    //    // After deleting, fetch the data of current page from server again
    //    console.log("ddddd");
    //    $.post("ServiceProviders/Home/JobQuote", { id: job.Id() }, function () {
    //        ServiceProviders.fetchDataOfCurrentPage(self.jobList)
    //    });
    //}



    self.quoteJob = function (job) {
        // After deleting, fetch the data of current page from server again
        console.log("ddddd");
        self.isDetail(true);
        self.modalStatus("Job Quotes");
        //ServiceProviders.mapJobToJobModel(job, self.job());
        //ko.mapping.fromJS(job, self.job);
        //});

    }



    self.detail = function (job) {
        self.modalStatus("View Detail");
        self.isDetail(true);
        ko.mapping.fromJS(job, self.job);
        //ServiceProviders.mapJobToJobModel(job, self.job());
    }
};

