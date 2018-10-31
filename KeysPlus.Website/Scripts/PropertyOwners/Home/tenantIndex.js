var Tenant = function (tenant) {

    //Fields and model declarations
    var self = this;
    var token = $("input[name = '__RequestVerificationToken']").val();
    var editUrl = $("#editUrl").val();
    var deleteUrl = $("#deleteUrl").val();
    var createUrl = $("#createUrl").val();
    var updatePhotoUrl = $("#updatePhoto").val();
    var forCreation = {};
    var PhotoProfile;
    var ImageUrl;
    

    self.templateName = ko.observable("tenantTemplate");
    self.URLData = {}; // Used as data for sending AJAX
    
    self.selectedTenantJob = ko.observable();
    self.deletedTenant = ko.observable();
    self.tenantTemplate = ko.observable("tenantIndex"); // Used to set the view for template
    self.tenantJobs = ko.observableArray();
    self.tenant = ko.observable();

    tenant.forEach(function (item) {
        self.tenantJobs.push(new TenantViewModel(item));
    });
    console.log(self.tenant());
    console.log(self.tenantJobs());
    (function () {
        forCreation = {
            Id: 0,
            TenantId: 0,
            PropertyName: "propertyName",
            JobDescription: "",
            JobStatusId: 0 ,
            CreatedOn: "" 

        };
        self.tenant(new TenantViewModel(forCreation));
        console.log(self.tenant());
    })();

    
    self.addNewJob = function (job) {
        console.log(job);
        $.ajax({
            url: "/PropertyOwners/Home/TenantCreateJob",
            data: ko.toJSON(job.JobDescription),
            method: "POST",
            contentType: 'application/json',

        }).done(function (data) {
            console.log(data.success);
            if (data.success) {
            }
            else {
                alert("Somthing went wrong!")
            }
        });
    };
    self.acceptJob = function (job) {
        
           /* $.get($(this).attr('id'), function (d) {
                self.selectedJob(job);
                console.log(job);
                console.log(self.selectedJob(job));
                ko.mapping.fromJS(job, self.tenantJobs);
                $('.row display-table-row').prepend(d);*/

                $('#tenantJobEditModal').appendTo("body").modal('show');
                $('#tenantJobEditModal').appendTo("body").off("hidden.bs.modal");
                $('#tenantJobEditModal').appendTo("body").on("hidden.bs.modal", function () {
                    //  self.selectedJob(null);
                });
           // });
        };
    
    self.saveJob = function (job) {

        console.log(job.selectedTenantJob());
            
        $.ajax({
            url: "/Jobs/Home/SaveJob",
            data: ko.toJSON(job.selectedTenantJob()),
            method: "POST",
            contentType: 'application/json',

        }).done(function (data) {
            alert(data);
        });

    };
    self.deleteTenantJob = function (data) {

        console.log(data);
        $.ajax({
            url: deleteUrl,
            type: 'post',
            headers: {
                "__RequestVerificationToken": token
            },
            data: { id: data.Id() }
        }).done(function (result) {
            console.log(result);
            if (result.success) {

            }
        });
    };

    self.goToIndex = ko.computed({
        read: function () {


            return self.tenantTemplate("tenantIndex");

        },
        write: function (data) {

            return self.tenantTemplate("tenantIndex");

        }
    });
    self.viewDetails = ko.computed({
        read: function () {
            return self.tenantTemplate();
        },
        write: function (data) {
            console.log(data);
            self.selectedTenantJob(data);
            return self.tenantTemplate("tenantJobDetails");
        }
    });
    self.editTenantJobRequest = ko.computed({
        read: function () {
            return self.tenantTemplate();
        },
        write: function (data) {

            console.log(data);
            self.selectedTenantJob(data);
            return self.tenantTemplate("tenantJobEdit");
        }
    });
    self.addTenantJob = ko.computed({
        read: function () {
            return self.tenantTemplate();
        },
        write: function (data) {
            console.log(data());
            self.selectedTenantJob(data);
            return self.tenantTemplate("tenantCreateJob");
        }
    });
    
};




function TenantViewModel(item) {

   
    console.log(item);
    var self = this;

    
    var dirtyObservable;

    ko.mapping.defaultOptions().include = [];
    ko.validation.init({
        grouping: {
            deep: true,
            observable: true,
            live: true
        }
    });
    ko.extenders.dirtyTrack = function (target, dirtyObservable) {
        target.subscribe(function () {
            dirtyObservable(true);

        });
        return target;
    };

    self.viewModel = ko.mapping.fromJS(item);
    self.validationModel = ko.validatedObservable(self.viewModel);

   
    self.viewModel.TenantId = item.TenantId;
    self.viewModel.PropertyId = item.PropertyId;
    self.viewModel.JobStatusId = item.JobStatusId;
    self.viewModel.JobDescription = item.JobDescription;
    self.viewModel.PropertyName = item.PropertyName["0"].name;
    self.viewModel.CreatedOn = item.CreatedOn;
    self.viewModel.disableItem = ko.observable(true);
    self.viewModel.dirty = ko.observable(false);
  
};

