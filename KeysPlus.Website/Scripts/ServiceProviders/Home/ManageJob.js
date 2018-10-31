$(function () {

    // view model
    var vm = {};


    function Job() {
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

    var jobListData = $("#jobListData").val();
    var jobListDataObject = $.parseJSON(jobListData);

    //job list model
    if (jobListDataObject.length == 1 && jobListDataObject[0].Id == -1) {
        vm.jobList = {};
    } else {
        vm.jobList = ko.mapping.fromJSON(jobListData);
        vm.jobList().forEach(function (job) {
            job.JobStartDate(moment(job.JobStartDate()).format().split('T')[0]);
        });
    }

    //job model
    vm.job = ko.observable(new Job());


    // view model properties
    vm.isEditing = ko.observable();
    vm.isDetail = ko.observable(false);
    vm.modalStatus = ko.observable();

    //dropdown list for property and job status
    $.ajax({
        url: "ServiceProviders/Home/GetPropertyByUserId",
        data: { userId: 1 },
        async: false,
        method: "POST",
        success: function (data) {
            vm.properties = ko.mapping.fromJS(data);
        }
    });

    vm.jobStatus = [
        { Id: 1, Code: "Finished", Name: "Finished" },
        { Id: 2, Code: "Inprocess", Name: "Inprocess" },
        { Id: 3, Code: "Pending", Name: "Pending" }, ]
    console.log(jobListDataObject);

    // behaviors 
    vm.addJob = function () {
        vm.isEditing(false);
        vm.modalStatus("Create new job");
        vm.isDetail(false);
        vm.job(new Job());
    };


    vm.editJob = function (job) {
        vm.isEditing(true);
        vm.modalStatus("Edit job");
        vm.isDetail(false);
        mapJobToJobMdel(job);
    };

    vm.detail = function (job) {
        vm.modalStatus("View Detail");
        vm.isDetail(true);
        mapJobToJobMdel(job);
    }

    function mapJobToJobMdel(job) {
        vm.job().Id(job.Id());
        vm.job().PropertyId(job.PropertyId());
        vm.job().PropertyName(job.PropertyName());
        vm.job().ProviderId(job.ProviderId());
        vm.job().ProviderName(job.ProviderName());
        vm.job().JobStartDate(job.JobStartDate());
        vm.job().JobEndDate(job.JobEndDate());
        vm.job().JobStatusId(job.JobStatusId());
        vm.job().JobStatusName(job.JobStatusName());
        vm.job().JobDescription(job.JobDescription());
        vm.job().CreatedOn(job.CreatedOn());
        vm.job().UpdatededOn(job.UpdatededOn());
        vm.job().PaymentAmount(job.PaymentAmount());
    };

    vm.save = function () {
        if (vm.isEditing()) {
            editJob();
        } else {
            createJob();
        }
    }

    ko.applyBindings(vm);

    function createJob() {
        $.post("ServiceProviders/Home/Save", vm.job(), function () {
            vm.jobList().push(vm.job());
        });
    }

    function editJob() {
        var aTag = $(".pagination a[href]");
        var url = aTag.prop("href");
        var page = url.substring(url.indexOf("=") + 1);
        if (aTag.prop("rel") == "next") {
            page = (parseInt(page) - 1);
        } else {
            page = (parseInt(page) + 1);
        }
        // After editing, fetch the data of current page from server again
        $.post("ServiceProviders/Home/Save", vm.job(), function () {
            $.get(url.substring(0, url.indexOf("=") + 1) + page, null, function (response) {
                $("#pagination").html(response);
                ko.mapping.fromJSON($("#jobListData").val(), vm.jobList);
                vm.jobList().forEach(function (job) {
                    job.JobStartDate(moment(job.JobStartDate()).format().split('T')[0]);
                });
            });
        });
    }

    $("#pagination").on("click", ".pagination a", function (event) {
        event.preventDefault();
        $.get($(this).prop("href"), null, function (response) {
            $("#pagination").html(response);
            ko.mapping.fromJSON($("#jobListData").val(), vm.jobList);
            vm.jobList().forEach(function (job) {
                job.JobStartDate(moment(job.JobStartDate()).format().split('T')[0]);
            });
        });
    })
})
